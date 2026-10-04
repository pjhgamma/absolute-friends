using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class RevivalBump : CosmeticSprite
{
    private const int RingDuration = 60;

    private const int Lifetime = 280;

    private readonly Creature _friend;

    private int _counter;

    private float _radius = 2000f;

    private float _lastRadius = 2000f;

    private float _radiusSpeed = -200.2f;

    public RevivalBump(Creature friend)
    {
        _friend = friend;
        pos = friend.mainBodyChunk.pos;
        lastPos = pos;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!Config.BumpEffects.IsActive || _friend.dead || _friend.slatedForDeletetion || _friend.room != room || _counter >= Lifetime)
        {
            Destroy();

            return;
        }

        pos = _friend.mainBodyChunk.pos;
        _lastRadius = _radius;

        if (_counter == 0)
        {
            room.PlaySound(SoundID.MENU_Karma_Ladder_Reinforcement_Dissipate_A, 0f, 1f, 1f);
        }
        if (_counter < RingDuration)
        {
            _radius += _radiusSpeed;
            _radiusSpeed *= 0.9f;
        }
        if (++_counter == RingDuration)
        {
            room.PlaySound(SoundID.UI_Multiplayer_Player_Revive, 0f, 1f, 1f);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        string symbol = _friend.abstractCreature.Icon.Symbol;
        Color color = _friend.abstractCreature.ShortcutColor;

        sLeaser.sprites =
        [
            new FSprite("Futile_White") { shader = RainWorldUtils.Shader("VectorCircleFadable") ?? FShader.defaultShader, alpha = 0f },
            new FSprite("Futile_White") { shader = RainWorldUtils.Shader("FlatLight") ?? FShader.defaultShader, color = Color.black, alpha = 0f },
            new FSprite(symbol) { color = color, alpha = 0f }
        ];

        AddToContainer(sLeaser, rCam, rCam.HudContainer);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (sLeaser.deleteMeNextFrame)
        {
            return;
        }

        Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
        float counter = Mathf.Max(0f, _counter - 1f + timeStacker);
        float ringLife = 1f - Mathf.InverseLerp(0f, RingDuration, counter);
        float radius = Mathf.Max(1f, Mathf.Lerp(_lastRadius, _radius, timeStacker));
        float alpha = counter < 100f
            ? Mathf.Sin(Mathf.InverseLerp(RingDuration, 100f, counter) * Mathf.PI)
            : Mathf.InverseLerp(Lifetime, 170f, counter);
        float fade = Mathf.Pow(Mathf.Max(0f, alpha), 0.7f);
        FSprite ring = sLeaser.sprites[0];
        FSprite glow = sLeaser.sprites[1];
        FSprite symbol = sLeaser.sprites[2];
        Color color = _friend.abstractCreature.ShortcutColor;

        ring.SetPosition(position);
        ring.isVisible = counter < RingDuration;
        ring.scale = radius / 8f;
        ring.alpha = ringLife * 6f / radius;
        ring.color = new Color(0f, 0f, Mathf.Sqrt(ringLife) * 0.25f);
        glow.SetPosition(position + new Vector2(0f, 20f));
        glow.scale = Mathf.Lerp(80f, 110f, fade) / 16f;
        glow.alpha = 0.17f * fade * fade;
        symbol.SetPosition(new Vector2(position.x, Mathf.Min(position.y + 10f + Custom.SCurve(Mathf.InverseLerp(100f, 190f, counter), 0.8f) * 80f, rCam.sSize.y - 30f)));
        symbol.color = counter is > RingDuration and < 100f && _counter % 6 < 2 ? Color.Lerp(color, Color.white, 0.8f) : color;
        symbol.alpha = fade;
    }
}
