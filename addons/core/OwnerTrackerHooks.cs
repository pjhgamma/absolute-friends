using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Core;

internal class OwnerTrackerHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat, Config.FriendCreature, Config.FriendNeutralCreature, Config.FriendGrabbed];

    protected override bool IsOptionEnabled => base.IsOptionEnabled || CreatureRules.HasConfiguredRules;

    protected override string? Subject => "Owner Tracker";

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ctor))]
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        OwnerUtils.ClearOwners();
    }
}
