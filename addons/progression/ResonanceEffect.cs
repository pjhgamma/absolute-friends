using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Progression;

internal class ResonanceEffect : UpdatableAndDeletable
{
    private readonly RoomSettings.RoomEffect? _roomEffect;

    private readonly bool _isAdded = true;

    private readonly float _amount;

    private float _counter = 1f;

    private bool _restored;

    public ResonanceEffect(Room room)
    {
        foreach (var effect in room.roomSettings?.effects ?? [])
        {
            if (effect.type == RoomSettings.RoomEffect.Type.VoidMelt)
            {
                _roomEffect = effect;
                _isAdded = false;
                _amount = effect.amount;

                break;
            }
        }

        if (_roomEffect == null)
        {
            _roomEffect = new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.VoidMelt, 0f, false)
            {
                save = false
            };
            room.roomSettings?.effects?.Add(_roomEffect);

            foreach (var camera in room.game?.cameras ?? [])
            {
                if (camera?.room == room)
                {
                    camera.SetUpFullScreenEffect("Bloom");
                    camera.fullScreenEffect.shader = room.game?.rainWorld?.Shaders["LevelMelt2"];
                    camera.lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.VoidMelt;
                    camera.fullScreenEffect.alpha = 1f;
                }
            }
        }

        for (int i = 0; i < 20; ++i)
        {
            room.AddObject(new MeltLights.MeltLight(1f, room.RandomPos(), room, Palette.Primary));
        }

        room.PlaySound(SoundID.SB_A14, 0f, 1f, 1f);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        _counter = Mathf.Max(0f, _counter - 1f / 60f);
        _roomEffect?.amount = Mathf.Lerp(_amount, 1f, Custom.SCurve(_counter, 0.6f));

        if (_counter == 0f)
        {
            Destroy();
        }
    }

    public override void Destroy()
    {
        Restore();

        base.Destroy();
    }

    private void Restore()
    {
        if (_restored || _roomEffect == null)
        {
            return;
        }

        _restored = true;

        if (_isAdded)
        {
            room?.roomSettings?.effects?.Remove(_roomEffect);
        }
        else
        {
            _roomEffect.amount = _amount;
        }

        foreach (var camera in room?.game?.cameras ?? [])
        {
            if (camera != null && camera.room == room)
            {
                camera.ApplyPalette();
            }
        }
    }
}
