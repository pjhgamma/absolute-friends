using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Iterators;

internal class MarkOverlay(Creature friend) : CosmeticSprite
{
    private const float Height = 30f;

    private const float MarkSize = 5f;

    private float _alpha;

    private float _lastAlpha;

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!Config.Mark.IsActive || friend.room != room || !MarkHooks.CanBearMark(friend))
        {
            Destroy();

            return;
        }

        BodyChunk head = friend.bodyChunks[0];

        lastPos = pos;
        pos = head.pos + new Vector2(0f, head.rad + Height);
        _lastAlpha = _alpha;
        _alpha = MarkHooks.MarkAlpha(friend);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new FSprite("Futile_White", true)
            {
                shader = RainWorldUtils.Shader("FlatLight") ?? FShader.defaultShader,
                alpha = 0f
            },
            new FSprite("pixel", true)
            {
                scale = MarkSize,
                alpha = 0f
            }
        ];

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
        float alpha = Mathf.Lerp(_lastAlpha, _alpha, timeStacker);
        FSprite glow = sLeaser.sprites[0];
        FSprite mark = sLeaser.sprites[1];
        Color color = friend.ShortCutColor();

        glow.SetPosition(position);
        glow.color = color;
        glow.alpha = 0.2f * alpha;
        glow.scale = 1f + alpha;
        mark.SetPosition(position);
        mark.color = color;
        mark.alpha = alpha;
    }
}
