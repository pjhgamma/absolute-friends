using RippleFriends.Core;
using RippleFriends.Utils;
using Watcher;

namespace RippleFriends.Iterators;

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
        if (abstractCreature is not AbstractOwner abstractOwner)
        {
            return null;
        }

        return abstractOwner.PhysicalObject switch
        {
            Oracle => RainWorldUtils.CurrentGame is not { IsStorySession: true } game
                || game.TimelinePoint != SlugcatStats.Timeline.Saint
                || abstractSlugcat.realizedObject is not Player { monkAscension: true },
            SLOracleSwarmer => true,
            Prince => true,
            _ => null
        };
    }
}
