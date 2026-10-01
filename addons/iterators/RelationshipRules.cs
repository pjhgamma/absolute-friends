using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using MoreSlugcats;
using Watcher;

namespace AbsoluteFriends.Iterators;

internal static class RelationshipRules
{
    public static void Register()
    {
        FriendUtils.RegisterFriendshipRule(IsFriendIterator);
        OwnerUtils.RegisterSelfOwnershipRule(IsSelfOwnedIterator);
    }

    public static void Unregister()
    {
        FriendUtils.UnregisterFriendshipRule(IsFriendIterator);
        OwnerUtils.UnregisterSelfOwnershipRule(IsSelfOwnedIterator);
    }

    private static bool IsSelfOwnedIterator(PhysicalObject physicalObject) => physicalObject is Oracle or SLOracleSwarmer or Prince;

    private static bool? IsFriendIterator(AbstractCreature abstractCreature, AbstractCreature abstractSlugcat)
    {
        if (abstractCreature.creatureTemplate?.type == CreatureTemplate.Type.Overseer)
        {
            return Config.Overseer.IsActive ? true : null;
        }

        if (abstractCreature is not AbstractOwner abstractOwner)
        {
            return null;
        }

        if (
            abstractOwner.PhysicalObject is Oracle
            && RainWorldUtils.CurrentGame is { IsStorySession: true } game
            && game.TimelinePoint == SlugcatStats.Timeline.Saint
            && abstractSlugcat.realizedObject is Player { monkAscension: true }
        )
        {
            return false;
        }

        return abstractOwner.PhysicalObject switch
        {
            Oracle oracle when oracle.ID == Oracle.OracleID.SL => Config.Moon.IsActive,
            SLOracleSwarmer => Config.MoonNeuron.IsActive,
            Oracle oracle when oracle.ID == Oracle.OracleID.SS => Config.Pebbles.IsActive,
            HalcyonPearl => Config.PebblesPearl.IsActive,
            Prince => Config.Prince.IsActive,
            _ => null
        };
    }
}
