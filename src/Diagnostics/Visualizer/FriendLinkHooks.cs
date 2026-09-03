using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Diagnostics.Visualizer;

internal class FriendLinkHooks : RoomOverlayHooks<FriendLinkOverlay>
{
    protected override Configurable<bool>[] Options => [Config.FriendLink];

    protected override FriendLinkOverlay Create(Room room) => new();

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        FriendLinkOverlay.Clear(self);

        orig(self);
    }
}
