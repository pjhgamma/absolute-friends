using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;
using VoidSea;

namespace AbsoluteFriends.Pilgrimage;

internal static class VoidSeaUtils
{
    public const float DepthHeight = 100f;

    private const float FollowRadius = 60f;

    private const float SnapDistance = 1000f;

    private const float DiveDepth = 40f;

    private const float PullFactor = 0.02f;

    private const float MaxPull = 3f;

    private const float RowSpacing = 40f;

    private const int ReleaseTime = 400;

    private static readonly ConditionalWeakTable<VoidSeaScene, VoidSeaState> _states = new();

    public static VoidSeaScene? ActiveScene(Room? room)
    {
        return room?.updateList.OfType<VoidSeaScene>().FirstOrDefault(scene => !scene.Inverted);
    }

    public static VoidSeaScene? CompanionScene(Creature creature)
    {
        if (creature is Player { isNPC: false } || ActiveScene(creature.room) is not { } scene)
        {
            return null;
        }

        VoidSeaState state = scene.State;

        if (creature.abstractCreature.IsCompanion)
        {
            RememberFriend(scene, state, creature);
        }

        return state.Owners.ContainsKey(creature) ? scene : null;
    }

    public static bool IsWormRider(Player player)
    {
        return !player.isNPC
            && player.abstractCreature?.IsTracked == true
            && ActiveScene(player.room) is { IsWormRiding: true } scene
            && Lead(scene.room.game) is { } lead
            && player != lead;
    }

    public static Player? FollowedPlayer(Creature creature, VoidSeaState state)
    {
        return creature.Carrier ?? NearestPlayer(creature, (abstractPlayer, _) => state.CanFollow(creature, abstractPlayer));
    }

    public static Player? NearestDiver(Creature friend, VoidSeaState state)
    {
        return NearestPlayer(friend, (abstractPlayer, player) =>
            !player.dead
            && (friend.abstractCreature.IsTrackedFor(abstractPlayer) || state.CanFollow(friend, abstractPlayer))
            && (player.inVoidSea || (player.Submersion >= 0.5f && player.mainBodyChunk.pos.y <= friend.mainBodyChunk.pos.y - DiveDepth)));
    }

    public static void Shift(PhysicalObject physicalObject, Vector2 delta)
    {
        foreach (var chunk in physicalObject.bodyChunks)
        {
            chunk.pos += delta;
            chunk.lastPos += delta;
            chunk.lastLastPos += delta;
        }

        foreach (var part in physicalObject.graphicsModule?.bodyParts ?? [])
        {
            part.pos += delta;
            part.lastPos += delta;
        }

        LightSource? light = physicalObject.graphicsModule switch
        {
            PlayerGraphics playerGraphics => playerGraphics.lightSource,
            LizardGraphics lizardGraphics => lizardGraphics.lightSource,
            _ => null
        };

        light?.HardSetPos(light.Pos + delta);

        if (DrawPositions(physicalObject) is { } drawPositions)
        {
            for (int i = 0; i < drawPositions.GetLength(0); ++i)
            {
                for (int j = 0; j < drawPositions.GetLength(1); ++j)
                {
                    drawPositions[i, j] += delta;
                }
            }
        }
    }

    public static void Stop(PhysicalObject physicalObject)
    {
        foreach (var chunk in physicalObject.bodyChunks)
        {
            chunk.vel = Vector2.zero;
        }
    }

    public static void FollowWormRide(VoidSeaScene scene, VoidSeaState state, Creature[] companions, HashSet<Creature> riders)
    {
        if (!scene.IsWormRiding || Lead(scene.room.game) is not { } lead || lead.room != scene.room)
        {
            return;
        }

        Vector2 leadPos = lead.mainBodyChunk.pos;
        Vector2 velocity = lead.mainBodyChunk.vel;
        int slot = 0;
        HashSet<PhysicalObject> moved = [];

        foreach (var player in OtherTrackedPlayers(scene, lead))
        {
            Vector2 delta = leadPos + new Vector2(RowOffset(slot++), 0f) - player.mainBodyChunk.pos;

            Carry(player, delta, velocity);

            foreach (var grasp in player.grasps)
            {
                if (grasp?.grabbed is { } held && held.room == scene.room && moved.Add(held))
                {
                    Carry(held, delta, velocity);

                    if (held is Creature creature)
                    {
                        riders.Add(creature);
                    }
                }
            }

            foreach (var companion in companions)
            {
                if (companion is Player { onBack: var carrier } && carrier == player && moved.Add(companion))
                {
                    Carry(companion, delta, velocity);

                    riders.Add(companion);
                }
            }
        }

        foreach (var companion in companions)
        {
            if (!companion.IsCarried && moved.Add(companion))
            {
                Carry(companion, leadPos + new Vector2(RowOffset(slot++), 0f) - companion.mainBodyChunk.pos, velocity);
            }

            riders.Add(companion);
        }

        if (scene.MainWormBehavior is { } behavior && behavior.phase == VoidWorm.MainWormBehavior.Phase.DepthReached && behavior.timeInPhase >= ReleaseTime)
        {
            state.RideReleased = true;
        }
    }

    public static void FollowDiver(Creature friend, Player diver)
    {
        if (friend.mainBodyChunk.pos.y < DepthHeight && diver.mainBodyChunk.pos.y < friend.mainBodyChunk.pos.y - DiveDepth)
        {
            StopRising(friend);
        }

        if (friend is Lizard { dead: false } lizard)
        {
            GuideLizard(lizard, diver);
        }

        Vector2 offset = diver.mainBodyChunk.pos - friend.mainBodyChunk.pos;
        float distance = offset.magnitude;

        if (distance <= FollowRadius)
        {
            return;
        }

        Vector2 direction = offset / distance;

        if (distance > SnapDistance && diver.inVoidSea)
        {
            Shift(friend, offset - direction * FollowRadius);
            Stop(friend);

            return;
        }

        Vector2 pull = direction * Mathf.Min(MaxPull, (distance - FollowRadius) * PullFactor);

        if (friend is Lizard or Player { isNPC: true })
        {
            friend.bodyChunks[0].vel += pull;

            return;
        }

        foreach (var chunk in friend.bodyChunks)
        {
            chunk.vel += pull;
        }
    }

    public static void StopRising(Creature creature)
    {
        foreach (var chunk in creature.bodyChunks)
        {
            chunk.vel.y = Mathf.Min(chunk.vel.y, 0f);
        }
    }

    public static void AttachThreads(VoidSeaScene scene, VoidSeaState state, Creature[] companions)
    {
        VoidWorm? worm = scene.worms.FirstOrDefault(worm => worm.mainWorm && worm.arms.Any(HasThread));

        if (worm?.arms.FirstOrDefault(HasThread) is not { } arm)
        {
            return;
        }

        IEnumerable<Creature> riders = Lead(scene.room.game) is { } lead && lead.room == scene.room
            ? companions.Concat(OtherTrackedPlayers(scene, lead))
            : companions;

        foreach (var rider in riders)
        {
            if (state.Threads.TryGetValue(rider, out WormThreadOverlay existing) && !existing.slatedForDeletetion)
            {
                continue;
            }

            WormThreadOverlay overlay = new(scene, worm, arm, rider);

            state.Threads[rider] = overlay;
            scene.room.AddObject(overlay);
        }
    }

    private static float RowOffset(int slot)
    {
        int step = slot / 2 + 1;

        return slot % 2 == 0 ? -step * RowSpacing : step * RowSpacing;
    }

    private static Player? Lead(RainWorldGame game)
    {
        return ModManager.CoopAvailable ? game.RealizedPlayerFollowedByCamera : game.FirstRealizedPlayer;
    }

    private static bool IsInMap(AImap aiMap, IntVector2 tile)
    {
        return tile.x >= 0 && tile.x < aiMap.width && tile.y >= 0 && tile.y < aiMap.height;
    }

    private static bool HasThread(VoidWorm.Arm arm)
    {
        return arm.thread != null && arm.threadStatus > 0;
    }

    private static Vector2[,]? DrawPositions(PhysicalObject physicalObject) => physicalObject.graphicsModule switch
    {
        PlayerGraphics playerGraphics => playerGraphics.drawPositions,
        LizardGraphics lizardGraphics => lizardGraphics.drawPositions,
        _ => null
    };

    private static void RememberFriend(VoidSeaScene scene, VoidSeaState state, Creature friend)
    {
        if (friend.room != scene.room || friend is Player { isNPC: false })
        {
            return;
        }

        if (!state.Owners.TryGetValue(friend, out HashSet<AbstractCreature> owners))
        {
            state.Owners[friend] = owners = [];
        }

        foreach (var abstractPlayer in scene.room.game.Players)
        {
            if (friend.abstractCreature.IsTrackedFor(abstractPlayer))
            {
                owners.Add(abstractPlayer);
            }
        }
    }

    private static IEnumerable<Player> OtherTrackedPlayers(VoidSeaScene scene, Player lead)
    {
        foreach (var abstractPlayer in scene.room.game.Players)
        {
            if (abstractPlayer.IsTracked && abstractPlayer.realizedCreature is Player player && player != lead && player.room == scene.room)
            {
                yield return player;
            }
        }
    }

    private static Player? NearestPlayer(Creature creature, Func<AbstractCreature, Player, bool> isEligible)
    {
        Player? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var abstractPlayer in creature.room?.game?.Players ?? [])
        {
            if (abstractPlayer.realizedCreature is not Player player || player.room != creature.room || !isEligible(abstractPlayer, player))
            {
                continue;
            }

            float distance = (player.mainBodyChunk.pos - creature.mainBodyChunk.pos).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearest = player;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private static void Carry(PhysicalObject physicalObject, Vector2 delta, Vector2 velocity)
    {
        if (delta.sqrMagnitude >= 1f)
        {
            Shift(physicalObject, delta);
        }

        foreach (var chunk in physicalObject.bodyChunks)
        {
            chunk.vel = velocity;
        }
    }

    private static void GuideLizard(Lizard lizard, Player diver)
    {
        if (lizard.AI?.pathFinder is not { } pathFinder || lizard.room?.aimap is not { } aiMap)
        {
            return;
        }

        IntVector2 sourceTile = lizard.room.GetTilePosition(lizard.mainBodyChunk.pos);
        IntVector2 targetTile = lizard.room.GetTilePosition(diver.mainBodyChunk.pos);

        if (!IsInMap(aiMap, sourceTile) || !IsInMap(aiMap, targetTile))
        {
            return;
        }

        WorldCoordinate destination = lizard.room.GetWorldCoordinate(targetTile);

        if (
            aiMap.TileAccessibleToCreature(lizard.mainBodyChunk.pos, lizard.Template)
            && aiMap.WorldCoordinateAccessibleToCreature(destination, lizard.Template)
            && pathFinder.CoordinateReachable(destination)
            && pathFinder.GetDestination != destination
        )
        {
            lizard.AI.SetDestination(destination);
        }
    }

    extension(AbstractCreature abstractCreature)
    {
        public bool IsCompanion => abstractCreature is { IsPlayer: false, IsTracked: true };
    }

    extension(Creature creature)
    {
        public bool IsCarried => creature.grabbedBy.Count > 0 || creature is Player { onBack: not null };

        public Player? Carrier
        {
            get
            {
                foreach (var abstractPlayer in creature.room?.game?.Players ?? [])
                {
                    if (
                        abstractPlayer.realizedCreature is Player player
                        && player.room == creature.room
                        && ((creature is Player { onBack: var carrier } && carrier == player) || creature.grabbedBy.Any(grasp => grasp.grabber == player))
                    )
                    {
                        return player;
                    }
                }

                return null;
            }
        }
    }

    extension(VoidSeaScene scene)
    {
        public VoidSeaState State => _states.GetOrCreateValue(scene);

        public VoidWorm? MainWorm => scene.worms.FirstOrDefault(worm => worm.mainWorm && !worm.slatedForDeletetion);

        public VoidWorm.MainWormBehavior? MainWormBehavior => scene.MainWorm?.behavior as VoidWorm.MainWormBehavior;

        public bool IsWormRiding => scene.ridingWorm && scene.MainWorm != null && !scene.State.RideReleased;

        public Creature[] GatherCompanions()
        {
            VoidSeaState state = scene.State;

            foreach (var abstractFriend in FriendUtils.TrackedFriends)
            {
                if (abstractFriend.realizedCreature is { } friend)
                {
                    RememberFriend(scene, state, friend);
                }
            }

            return [.. state.Owners.Keys.Where(friend => friend.room == scene.room && !friend.slatedForDeletetion)];
        }
    }
}
