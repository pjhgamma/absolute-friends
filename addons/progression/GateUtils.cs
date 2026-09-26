using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using Watcher;

namespace AbsoluteFriends.Progression;

internal static class GateUtils
{
    private static readonly ConditionalWeakTable<Player, GateIdleState> _idleStates = new();

    internal static void UpdatePlayerIdle(Player player)
    {
        GateIdleState idleState = _idleStates.GetOrCreateValue(player);
        int noInputCounter = player.touchedNoInputCounter;

        if (!player.Consious)
        {
            idleState.WasUnconscious = true;
        }
        else if (idleState.WasUnconscious)
        {
            idleState.WasUnconscious = false;
            idleState.ResetCounter = noInputCounter;
        }
        else if (noInputCounter < idleState.ResetCounter)
        {
            idleState.ResetCounter = 0;
        }
    }

    private static bool IsIdleForGate(Player player, float seconds)
    {
        return player.Consious
            && _idleStates.TryGetValue(player, out GateIdleState idleState)
            && !idleState.WasUnconscious
            && player.touchedNoInputCounter - idleState.ResetCounter > seconds * RainWorldUtils.Second;
    }

    extension(ShelterDoor shelterDoor)
    {
        public bool IsInShelterDoor(Creature creature)
        {
            return shelterDoor.room == creature.room
                && !shelterDoor.Broken
                && !creature.inShortcut
                && creature.abstractCreature is { } abstractCreature
                && abstractCreature.pos.Tile is { } creatureTile
                && shelterDoor.room is { } room
                && room.LocalCoordinateOfNode(0).Tile is { } nodeTile
                && Custom.ManhattanDistance(creatureTile, nodeTile) > 6
                && ShelterDoor.IsTileInsideShelterRange(room.abstractRoom, creatureTile)
                && (creature is not Player player || !player.stillInStartShelter);
        }
    }

    extension(RegionGate regionGate)
    {
        public float GetTile(float x)
        {
            return (x / 20f - (regionGate.room?.TileWidth ?? 0) / 2f) * (regionGate.letThroughDir ? 1f : -1f);
        }

        public float GetBodyChunkLeftTile(BodyChunk bodyChunk)
        {
            return regionGate.GetTile(bodyChunk.pos.x - bodyChunk.rad * (regionGate.letThroughDir ? 1f : -1f));
        }

        public float GetBodyChunkRightTile(BodyChunk bodyChunk)
        {
            return regionGate.GetTile(bodyChunk.pos.x + bodyChunk.rad * (regionGate.letThroughDir ? 1f : -1f));
        }

        public bool IsInRegionGate(Creature creature)
        {
            if (regionGate.room != creature.room || !regionGate.MeetRequirement)
            {
                return false;
            }

            foreach (var bodyChunk in creature.bodyChunks ?? [])
            {
                if (bodyChunk != null && (regionGate.GetBodyChunkLeftTile(bodyChunk) < -8f || regionGate.GetBodyChunkRightTile(bodyChunk) > 8f))
                {
                    return false;
                }
            }

            return true;
        }
    }

    extension(RoomSpecificScript.SB_A14KarmaIncrease karmaIncrease)
    {
        public bool IsInKarmaIncrease(Creature creature)
        {
            return karmaIncrease.room == creature.room && creature.firstChunk.pos.x < 550f;
        }
    }

    extension(WarpPoint warpPoint)
    {
        public bool IsInWarpPoint(Creature creature)
        {
            return warpPoint.room == creature.room
                && warpPoint.transportable
                && creature.mainBodyChunk?.pos is { } creaturePos
                && Custom.DistLess(warpPoint.pos, creaturePos, warpPoint.PullRadius);
        }
    }

    extension(Room? room)
    {
        public bool IsInGate(Creature creature)
        {
            if (room?.shelterDoor is { } shelterDoor && shelterDoor.IsInShelterDoor(creature))
            {
                return true;
            }

            if (room?.regionGate is { } regionGate && regionGate.IsInRegionGate(creature))
            {
                return true;
            }

            foreach (var updatableAndDeletable in room?.updateList ?? [])
            {
                bool isInGate = updatableAndDeletable switch
                {
                    RoomSpecificScript.SB_A14KarmaIncrease karmaIncrease => karmaIncrease.IsInKarmaIncrease(creature),
                    WarpPoint warpPoint => warpPoint.IsInWarpPoint(creature),
                    _ => false
                };

                if (isInGate)
                {
                    return true;
                }
            }

            return false;
        }
    }

    extension<T>(T gate) where T : UpdatableAndDeletable
    {
        public bool CanActivate(List<AbstractCreature>? abstractPlayers, Func<T, Creature, bool> isInGate)
        {
            bool canActivate = true;

            foreach (var abstractCreature in FriendUtils.TrackedFriends)
            {
                if (abstractCreature.realizedCreature is Creature creature && !creature.dead && !isInGate(gate, creature))
                {
                    if (!Config.GateForce.IsActive)
                    {
                        return false;
                    }

                    canActivate = false;

                    break;
                }
            }

            foreach (var abstractPlayer in abstractPlayers ?? [])
            {
                if (abstractPlayer.realizedCreature is not Player player || !player.IsPlayer)
                {
                    continue;
                }

                if (!isInGate(gate, player))
                {
                    return false;
                }

                if (player.onBack != null)
                {
                    continue;
                }

                float idleTime = Config.GateTime.Value + (Config.GateForce.IsActive && !canActivate ? Config.GateForceTime.Value : 0f);

                if (!IsIdleForGate(player, idleTime))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private sealed class GateIdleState
    {
        public bool WasUnconscious;

        public int ResetCounter;
    }
}
