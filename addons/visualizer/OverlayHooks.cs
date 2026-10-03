using System.Runtime.CompilerServices;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Visualizer;

internal abstract class RoomOverlayHooks<TOverlay> : BaseHooks
    where TOverlay : UpdatableAndDeletable
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
    where TOverlay : UpdatableAndDeletable
{
    private readonly ConditionalWeakTable<PhysicalObject, TOverlay> _overlays = new();

    protected abstract TOverlay Create(PhysicalObject target);

    [HookPatch(typeof(On.Room), nameof(On.Room.AddObject))]
    protected void On_Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
    {
        orig(self, obj);

        if (obj is PhysicalObject physicalObject)
        {
            OverlayUtils.Attach(_overlays, physicalObject, self, Create);
        }
    }
}

internal class FriendHudHooks : RoomOverlayHooks<FriendHudOverlay>
{
    protected override Configurable<bool>[] Options => [Config.FriendMarkers, Config.FriendMeter];

    protected override string? Subject => "Friend HUD";

    protected override FriendHudOverlay Create(Room room) => new(room);
}

internal class TagHooks : RoomOverlayHooks<TagOverlay>
{
    protected override Configurable<bool>[] Options => [Config.Icons, Config.Names];

    protected override string? Subject => "Relationship Tags";

    protected override TagOverlay Create(Room room) => new();
}

internal class FriendLinkHooks : RoomOverlayHooks<FriendLinkOverlay>
{
    protected override Configurable<bool>[] Options => [Config.Links];

    protected override FriendLinkOverlay Create(Room room) => new();

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        FriendLinkOverlay.Refresh(self);

        orig(self);
    }
}

internal class OwnerLinkHooks : ObjectOverlayHooks<OwnerLinkOverlay>
{
    protected override Configurable<bool>[] Options => [Config.Links];

    protected override OwnerLinkOverlay Create(PhysicalObject target) => new(target);
}
