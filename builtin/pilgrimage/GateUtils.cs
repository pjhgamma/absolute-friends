using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Pilgrimage;

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

    private sealed class GateIdleState
    {
        public bool WasUnconscious;

        public int ResetCounter;
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
}
