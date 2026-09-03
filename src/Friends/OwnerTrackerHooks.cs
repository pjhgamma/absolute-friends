using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Friends;

internal class OwnerTrackerHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat, Config.FriendCreature, Config.FriendNeutralCreature, Config.FriendGrabbed, Config.FriendIterator];

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ctor))]
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        OwnerUtils.ClearOwners();
    }
}
