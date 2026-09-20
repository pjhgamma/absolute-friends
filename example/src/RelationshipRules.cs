using RippleFriends.Core;
using RippleFriends.Options;
using RippleFriends.Utils;

namespace RippleFriendsExample;

internal static class RelationshipRules
{
    internal static void Register()
    {
        // Return null to defer to other rules and then Core's built-in decision; false vetoes friendship.
        FriendUtils.RegisterFriendshipRule(IsFriendHunterDaddy);
        FriendUtils.RegisterFriendshipRule(IsGreenNeuronFriend);

        // Self-owned objects participate in ownership checks without belonging to a creature.
        OwnerUtils.RegisterSelfOwnershipRule(IsGreenNeuronSelfOwned);
    }

    internal static void Unregister()
    {
        FriendUtils.UnregisterFriendshipRule(IsFriendHunterDaddy);
        FriendUtils.UnregisterFriendshipRule(IsGreenNeuronFriend);
        OwnerUtils.UnregisterSelfOwnershipRule(IsGreenNeuronSelfOwned);
    }

    private static bool? IsFriendHunterDaddy(AbstractCreature source, AbstractCreature target)
    {
        if (!AddonConfig.Friendship.IsActive || !source.IsHunterDaddy || !target.IsSlugcat)
        {
            return null;
        }

        return !AddonConfig.AggressiveHunter.IsActive || !target.IsRed;
    }

    private static bool? IsGreenNeuronFriend(AbstractCreature abstractCreature, AbstractCreature abstractSlugcat)
    {
        if (abstractCreature is not AbstractOwner abstractOwner)
        {
            return null;
        }

        return AddonConfig.GreenNeuronOwnership.IsActive && abstractOwner.PhysicalObject is NSHSwarmer ? true : null;
    }

    private static bool IsGreenNeuronSelfOwned(PhysicalObject physicalObject) => AddonConfig.GreenNeuronOwnership.IsActive && physicalObject is NSHSwarmer;
}
