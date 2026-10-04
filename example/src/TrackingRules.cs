using AbsoluteFriends.Core;
using AbsoluteFriends.Options;

namespace AbsoluteFriendsExample;

internal static class TrackingRules
{
    internal static void Register()
    {
        // A tracking rule returns false to exclude a creature. A creature stays tracked only while every rule returns true.
        // Untracked friends are still friends; other addons simply stop treating them as companions, for example at gates.
        // Players are always tracked, so rules are never asked about them.
        FriendUtils.RegisterTrackingRule(IsTrackedHunterDaddy);

        // A name rule returns a name, or null to let the next rule or the default name decide.
        // The first non-empty name wins, and rules run in registration order.
        // The Manager's nickname rule is one of these rules, so a Manager nickname can hide this name or be hidden by it, depending on which addon registered first.
        FriendDisplayNames.RegisterNameRule(HunterDaddyName);
    }

    internal static void Unregister()
    {
        FriendUtils.UnregisterTrackingRule(IsTrackedHunterDaddy);
        FriendDisplayNames.UnregisterNameRule(HunterDaddyName);
    }

    private static bool IsTrackedHunterDaddy(AbstractCreature abstractCreature) => !AddonConfig.UntrackedHunter.IsActive || !abstractCreature.IsHunterDaddy;

    // Name rules run whenever a name is drawn, so keep them cheap.
    private static string? HunterDaddyName(AbstractCreature abstractCreature) => AddonConfig.HunterName.IsActive && abstractCreature.IsHunterDaddy
        ? $"Player {Math.Abs(abstractCreature.ID.number)}"
        : null;
}
