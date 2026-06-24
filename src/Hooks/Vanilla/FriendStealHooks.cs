using static RippleFriends.Core.FriendTracker;
using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class FriendStealHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.NoStealing;

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        if (IsFriend(self, GetGrabber(obj)))
        {
            return false;
        }
        return orig(self, obj);
    }
}
