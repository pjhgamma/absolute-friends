using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using HUD;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class DeathBump : CosmeticSprite
{
    private const int FirstPing = 5;

    private const int SecondPing = 40;

    private const int Lifetime = 220;

    private readonly Creature _friend;

    private int _counter = -20;

    private float _alpha;

    private float _lastAlpha;

    private float _blink;

    private float _lastBlink;

    private bool _isHurried;

    public DeathBump(Creature friend)
    {
        _friend = friend;
        pos = Ping;
        lastPos = pos;
    }

    private Vector2 Ping => _friend.mainBodyChunk.pos + new Vector2(0f, 10f);

    public override void Update(bool eu)
    {
        base.Update(eu);

        RoomCamera? camera = RainWorldUtils.MainCamera(room?.game);

        if (!Config.BumpEffects.IsActive || camera == null || camera.room != room || _counter > Lifetime)
        {
            Destroy();

            return;
        }

        lastPos = pos;
        _lastAlpha = _alpha;
        _lastBlink = _blink;

        if (_friend.room == room && !_friend.slatedForDeletetion)
        {
            pos = Ping;
        }

        if (_counter < 0)
        {
            if (_friend.room != room || _friend.grabbedBy.Count > 0 || !camera.PositionCurrentlyVisible(_friend.mainBodyChunk.pos, 200f, true))
            {
                _counter = 0;
                _isHurried = true;
                camera.hud?.PlaySound(SoundID.UI_Multiplayer_Player_Dead_A);
                camera.hud?.PlaySound(SoundID.UI_Multiplayer_Player_Dead_B);
            }
            else if (_friend.bodyChunks.All(chunk => Custom.DistLess(chunk.pos, chunk.lastLastPos, 6f)))
            {
                ++_counter;
            }
        }

        _counter += _isHurried ? 11 : 1;

        if (_counter < SecondPing)
        {
            _alpha = Mathf.Sin(Mathf.InverseLerp(0f, SecondPing, _counter) * Mathf.PI);
            _blink = Custom.LerpAndTick(_blink, 1f, 0.07f, 1f / 30f);

            if (_counter == FirstPing)
            {
                Ring(camera, 10f, 10f, 0.82f, 30f, 1f);
                camera.hud?.PlaySound(SoundID.UI_Multiplayer_Player_Dead_A);
            }

            return;
        }

        if (_counter == SecondPing)
        {
            Ring(camera, 20f, 30f, 0.94f, 60f, 0.5f);
            camera.hud?.PlaySound(SoundID.UI_Multiplayer_Player_Dead_B);

            _alpha = 1f;
            _blink = 0f;

            return;
        }

        _alpha = Mathf.InverseLerp(Lifetime, 110f, _counter);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new FSprite("Futile_White") { shader = RainWorldUtils.Shader("FlatLight") ?? FShader.defaultShader, color = Color.black, alpha = 0f },
            new FSprite(_friend.abstractCreature.Icon.Symbol) { alpha = 0f },
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
        float fade = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(_lastAlpha, _alpha, timeStacker)), 0.7f);
        Color color = _friend.abstractCreature.ShortcutColor;
        FSprite glow = sLeaser.sprites[0];
        FSprite symbol = sLeaser.sprites[1];

        if (_counter % 6 < 2 && _lastBlink > 0f)
        {
            color = Color.Lerp(color, Color.white, Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(_lastBlink, _blink, timeStacker)));
        }

        glow.SetPosition(position + new Vector2(0f, 10f));
        glow.scale = Mathf.Lerp(80f, 110f, fade) / 16f;
        glow.alpha = 0.17f * fade * fade;
        symbol.SetPosition(new Vector2(position.x, Mathf.Min(position.y + Custom.SCurve(Mathf.InverseLerp(SecondPing, 130f, _counter + timeStacker), 0.8f) * 80f, rCam.sSize.y - 30f)));
        symbol.color = color;
        symbol.alpha = fade;
    }

    private void Ring(RoomCamera camera, float radius, float speed, float slowDown, float lifetime, float alpha)
    {
        if (_isHurried || camera.hud is not { } hud)
        {
            return;
        }

        hud.fadeCircles.Add(new FadeCircle(hud, radius, speed, slowDown, lifetime, 4f, _friend.mainBodyChunk.pos - camera.pos, hud.fContainers[1])
        {
            alphaMultiply = alpha,
            fadeThickness = alpha >= 1f,
        });
    }
}
