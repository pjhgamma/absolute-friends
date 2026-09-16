using RippleFriends.Hooks;
using System.Runtime.CompilerServices;

namespace RippleFriends.Core.Visualizer;

internal abstract class RoomOverlayHooks<TOverlay> : BaseHooks
    where TOverlay : UpdatableAndDeletable, IOverlay
{
    private readonly ConditionalWeakTable<Room, TOverlay> _overlays = new();

    protected abstract TOverlay Create(Room room);

    [HookPatch(typeof(On.Room), nameof(On.Room.Update))]
    protected void On_Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        OverlayUtils.Attach(_overlays, self, self, Create);
    }
}

internal abstract class ObjectOverlayHooks<TOverlay> : BaseHooks
    where TOverlay : UpdatableAndDeletable, IOverlay
{
    private readonly ConditionalWeakTable<UpdatableAndDeletable, TOverlay> _overlays = new();

    protected abstract TOverlay Create(UpdatableAndDeletable target);

    [HookPatch(typeof(On.Room), nameof(On.Room.AddObject))]
    protected void On_Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
    {
        orig(self, obj);

        if (obj is IOverlay)
        {
            return;
        }

        OverlayUtils.Attach(_overlays, obj, self, Create);
    }
}
