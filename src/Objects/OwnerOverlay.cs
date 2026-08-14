using UnityEngine;
using static RippleFriends.Core.OwnerTracker;

namespace RippleFriends.Objects;

internal abstract class OwnerOverlay(UpdatableAndDeletable updatableAndDeletable) : CosmeticSprite
{
    protected UpdatableAndDeletable Target => updatableAndDeletable;

    protected Creature? Owner { get; private set; }

    protected abstract IEnumerable<FNode> Nodes { get; }

    protected abstract Configurable<bool> Option { get; }

    public override void Update(bool eu)
    {
        base.Update(eu);

        try
        {
            Track();
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

        foreach (FNode node in nodes)
        {
            container.AddChild(node);
        }

        sLeaser.sprites = [.. nodes.OfType<FSprite>()];
        sLeaser.containers = [container];

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (sLeaser.containers is { Length: > 0 } containers && containers[0] is { } container)
        {
            (newContatiner ?? rCam.ReturnFContainer("HUD")).AddChild(container);

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
                Owner == null
                || !Option.Value
                || !LinkOverlayUtils.ShouldDraw(rCam, room)
                || !Target.TryGetPosition(timeStacker, out var position)
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

    protected virtual void OwnerChanged()
    {
    }

    private void Track()
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

        Creature? owner = GetOwner(Target);

        if (ReferenceEquals(owner, Target))
        {
            owner = null;
        }

        if (!ReferenceEquals(owner, Owner))
        {
            Owner = owner;

            OwnerChanged();
        }
    }
}
