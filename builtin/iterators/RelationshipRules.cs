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

    private static bool? Grant(Configurable<bool> option) => option.IsActive ? true : null;

    private static bool IsSelfOwnedIterator(PhysicalObject physicalObject) => physicalObject is Oracle or SLOracleSwarmer or HalcyonPearl or Prince;

    private static bool? IsFriendIterator(AbstractCreature abstractCreature, AbstractCreature abstractSlugcat)
    {
        if (abstractCreature.creatureTemplate?.type == CreatureTemplate.Type.Overseer)
        {
            return Grant(Config.Overseer);
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
            Oracle oracle when oracle.ID == Oracle.OracleID.SL || oracle.ID == MoreSlugcatsEnums.OracleID.DM => Grant(Config.Moon),
            SLOracleSwarmer => Grant(Config.MoonNeuron),
            Oracle oracle when oracle.ID == Oracle.OracleID.SS || oracle.ID == MoreSlugcatsEnums.OracleID.CL => Grant(Config.Pebbles),
            HalcyonPearl => Grant(Config.PebblesPearl),
            Prince => Grant(Config.Prince),
            _ => null
        };
    }
}
