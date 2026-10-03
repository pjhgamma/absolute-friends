using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class OwnerLinkOverlay(PhysicalObject target) : CosmeticSprite
{
    private const float OwnerWidth = 2.5f;

    private const float ObjectWidth = 0.5f;

    private readonly TriangleMesh _line = new("Futile_White", [new(0, 1, 2), new(1, 2, 3)], false, false)
    {
        color = Palette.Secondary,
        isVisible = true,
    };

    private readonly UpdateTimer _timer = new();

    private AbstractCreature? _owner;

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
        if (!OverlayUtils.ShouldDraw(rCam, room))
        {
            sLeaser.sprites = [];
            sLeaser.containers = [];

            return;
        }

        FContainer container = new()
        {
            isVisible = false,
        };

        container.AddChild(_line);

        _line.IsHologram = true;

        sLeaser.sprites = [_line];
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
                !Config.Links.IsActive
                || !Config.Owners.IsActive
                || !OverlayUtils.ShouldDraw(rCam, room)
                || _owner?.RealizedOwner is not PhysicalObject { slatedForDeletetion: false } owner
                || owner.room != room
                || !OverlayUtils.TryGetLine(rCam, target, owner, timeStacker, out Vector2 position, out Vector2 ownerPosition)
            )
            {
                container.isVisible = false;

                return;
            }

            BuildLine(ownerPosition - position, Vector2.zero, OwnerWidth, ObjectWidth);

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

    private void Follow()
    {
        if (target == null || target.slatedForDeletetion || target.room == null)
        {
            Destroy();

            return;
        }

        if (room != target.room)
        {
            room?.RemoveObject(this);
            target.room.AddObject(this);
        }

        if (_timer.Elapse())
        {
            _owner = target.ExternalOwner;
        }
    }

    private void BuildLine(Vector2 start, Vector2 end, float startWidth, float endWidth)
    {
        Vector2 unit = (end - start).normalized;
        Vector2 orthonormal = new(-unit.y, unit.x);

        _line.MoveVertice(0, start + (orthonormal * startWidth * 0.5f));
        _line.MoveVertice(1, start - (orthonormal * startWidth * 0.5f));
        _line.MoveVertice(2, end + (orthonormal * endWidth * 0.5f));
        _line.MoveVertice(3, end - (orthonormal * endWidth * 0.5f));
    }
}
