using AbsoluteFriends.Options;
using UnityEngine;

namespace AbsoluteFriends.Core.Visualizer;

internal abstract class TargetOverlay(UpdatableAndDeletable updatableAndDeletable) : CosmeticSprite, IOverlay
{
    protected UpdatableAndDeletable Target => updatableAndDeletable;

    protected abstract IEnumerable<FNode> Nodes { get; }

    protected abstract Configurable<bool> Option { get; }

    protected virtual bool IsVisible => true;

    public override void Update(bool eu)
    {
        base.Update(eu);

        try
        {
            Follow();
        }
        catch (Exception exception)
        {
            this.Destroy(exception);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        FContainer container = new()
        {
            isVisible = false,
        };

        List<FNode> nodes = [.. Nodes];

        foreach (var node in nodes)
        {
            container.AddChild(node);
        }

        sLeaser.sprites = [.. nodes.OfType<FSprite>()];
        sLeaser.containers = [container];

        AddToContainer(sLeaser, rCam, rCam.HudContainer);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (sLeaser.containers is { Length: > 0 } containers && containers[0] is { } container)
        {
            (newContatiner ?? rCam.HudContainer).AddChild(container);

            return;
        }

        base.AddToContainer(sLeaser, rCam, newContatiner);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (sLeaser.containers is not { Length: > 0 } containers || containers[0] is not { } container)
        {
            return;
        }

        try
        {
            if (
                !Option.IsActive
                || !IsVisible
                || !OverlayUtils.ShouldDraw(rCam, room)
                || !Target.TryGetPosition(timeStacker, out Vector2 position)
                || !Draw(position, rCam, timeStacker)
            )
            {
                container.isVisible = false;

                return;
            }

            container.isVisible = true;
            container.x = position.x - camPos.x;
            container.y = position.y - camPos.y;
        }
        catch (Exception exception)
        {
            container.isVisible = false;

            this.Destroy(exception);
        }
    }

    protected abstract bool Draw(Vector2 position, RoomCamera camera, float timeStacker);

    protected virtual void Follow()
    {
        if (Target == null || Target.slatedForDeletetion || Target.room == null)
        {
            Destroy();

            return;
        }

        if (room != Target.room)
        {
            room?.RemoveObject(this);
            Target.room.AddObject(this);
        }
    }
}
