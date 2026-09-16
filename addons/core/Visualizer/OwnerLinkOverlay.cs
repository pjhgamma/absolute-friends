using RippleFriends.Utils;
using UnityEngine;

namespace RippleFriends.Core.Visualizer;

internal class OwnerLinkOverlay(UpdatableAndDeletable target) : OwnerOverlay(target)
{
    private const float OwnerWidth = 2.5f;

    private const float ObjectWidth = 0.5f;

    private readonly TriangleMesh _line = new("Futile_White", [new(0, 1, 2), new(1, 2, 3)], false, false)
    {
        color = Palette.Secondary,
        isVisible = true,
    };

    protected override IEnumerable<FNode> Nodes => [_line];

    protected override Configurable<bool> Option => Config.OwnerLink;

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        _line.IsHologram = true;
    }

    protected override bool Draw(Vector2 position, RoomCamera camera, float timeStacker)
    {
        if (
            OwnerObject is not { slatedForDeletetion: false } owner
            || owner.room != room
            || !OverlayUtils.TryGetLine(camera, Target, owner, timeStacker, out _, out Vector2 second)
        )
        {
            return false;
        }

        BuildLine(second - position, Vector2.zero, OwnerWidth, ObjectWidth);

        return true;
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
