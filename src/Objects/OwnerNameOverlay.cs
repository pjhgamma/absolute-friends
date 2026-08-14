using RippleFriends.Options;
using UnityEngine;

namespace RippleFriends.Objects;

internal class OwnerNameOverlay(UpdatableAndDeletable target) : OwnerOverlay(target)
{
    private const float BackgroundHeight = 15f;

    private const float GapAboveObject = 20f;

    private readonly FLabel _text = new(RWCustom.Custom.GetDisplayFont(), "")
    {
        anchorX = 0.5f,
        anchorY = 0.5f,
        scale = 0.75f,
        y = GapAboveObject,
    };

    private readonly FSprite _background = new("pixel", true)
    {
        color = Color.black,
        alpha = 0.5f,
        anchorX = 0.5f,
        anchorY = 0.5f,
        scaleY = BackgroundHeight,
        y = GapAboveObject,
    };

    protected override IEnumerable<FNode> Nodes => [_background, _text];

    protected override Configurable<bool> Option => Config.OwnerName;

    protected override void OwnerChanged() => _text.text = Owner?.abstractCreature?.ToString() ?? Owner?.GetType().Name ?? "";

    protected override bool Draw(Vector2 position, RoomCamera camera, float timeStacker)
    {
        if (!position.IsOnScreen(camera))
        {
            return false;
        }

        _background.scaleX = _text.textRect.xMax - _text.textRect.xMin;

        return true;
    }
}
