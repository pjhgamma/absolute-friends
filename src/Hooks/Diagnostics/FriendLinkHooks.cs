using RippleFriends.Objects;
using RippleFriends.Options;
using System.Runtime.CompilerServices;

namespace RippleFriends.Hooks.Diagnostics;

internal class FriendLinkHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.FriendLink;

    private static readonly ConditionalWeakTable<Room, FriendLinkOverlay> _overlays = new();

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        FriendLinkOverlay.Clear(self);

        orig(self);
    }

    [HookPatch(typeof(On.Room), nameof(On.Room.Update))]
    private static void On_Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        if (_overlays.TryGetValue(self, out var overlay))
        {
            if (!overlay.slatedForDeletetion)
            {
                return;
            }

            _overlays.Remove(self);
        }

        overlay = new();

        _overlays.Add(self, overlay);

        self.AddObject(overlay);
    }
}
