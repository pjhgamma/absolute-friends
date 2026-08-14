using RippleFriends.Objects;
using RippleFriends.Options;
using System.Runtime.CompilerServices;

namespace RippleFriends.Hooks.Diagnostics;

internal class OwnerNameHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.OwnerName;

    private static readonly ConditionalWeakTable<UpdatableAndDeletable, OwnerNameOverlay> _overlays = new();

    [HookPatch(typeof(On.Room), nameof(On.Room.AddObject))]
    private static void On_Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
    {
        orig(self, obj);

        if (obj.IsOverlay())
        {
            return;
        }

        if (_overlays.TryGetValue(obj, out var overlay))
        {
            if (!overlay.slatedForDeletetion)
            {
                return;
            }

            _overlays.Remove(obj);
        }

        overlay = new(obj);

        _overlays.Add(obj, overlay);

        self.AddObject(overlay);
    }
}
