using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class FriendLinkOverlay : CosmeticSprite
{
    private const int InitialLines = 16;

    private const int SpareFactor = 4;

    private const float LineWidth = 1.5f;

    private const int QuietFrames = RainWorldUtils.Second;

    private static readonly List<(PhysicalObject First, PhysicalObject Second, FriendshipType Type)> _pairs = [];

    private static readonly List<PhysicalObject> _objects = [];

    private static readonly UpdateTimer _timer = new();

    private static Room? _scanned;

    private int _quiet;

    public static void Refresh(RainWorldGame? game)
    {
        Room? viewed = RainWorldUtils.MainCamera(game)?.room;

        if (!ReferenceEquals(viewed, _scanned))
        {
            _scanned = viewed;

            _timer.Reset();
        }

        if (_timer.Elapse())
        {
            Scan(viewed);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [];

        Fit(sLeaser, rCam, InitialLines);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner ?? rCam.HudContainer);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        int drawnCount = 0;

        try
        {
            if (Config.Links.IsActive && OverlayUtils.ShouldDraw(rCam, room))
            {
                Fit(sLeaser, rCam, _pairs.Count);

                foreach (var (firstObject, secondObject, type) in _pairs)
                {
                    if (
                        !Relationships.IsShown(type)
                        || firstObject.room != room
                        || secondObject.room != room
                        || !OverlayUtils.TryGetLine(rCam, firstObject, secondObject, timeStacker, out Vector2 first, out Vector2 second)
                    )
                    {
                        continue;
                    }

                    FSprite sprite = sLeaser.sprites[drawnCount++];

                    sprite.isVisible = true;
                    sprite.color = Relationships.ColorOf(false);
                    sprite.x = first.x - camPos.x;
                    sprite.y = first.y - camPos.y;
                    sprite.scaleY = Vector2.Distance(first, second);
                    sprite.rotation = Custom.AimFromOneVectorToAnother(first, second);
                }
            }
        }
        catch (Exception exception)
        {
            this.Destroy(exception);
        }

        for (int i = drawnCount; i < sLeaser.sprites.Length; ++i)
        {
            sLeaser.sprites[i].isVisible = false;
        }
    }

    private void Fit(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int count)
    {
        int size = sLeaser.sprites.Length;

        if (count > size)
        {
            _quiet = 0;

            int newSize = size;

            while (newSize < count)
            {
                newSize = Math.Max(newSize * 2, InitialLines);
            }

            Array.Resize(ref sLeaser.sprites, newSize);

            for (int i = size; i < newSize; ++i)
            {
                FSprite sprite = new("pixel", true)
                {
                    anchorY = 0f,
                    scaleX = LineWidth,
                    isVisible = false,
                    IsHologram = true
                };

                sLeaser.sprites[i] = sprite;
            }

            AddToContainer(sLeaser, rCam, rCam.HudContainer);

            return;
        }

        if (count * SpareFactor > size)
        {
            _quiet = 0;

            return;
        }

        if (size <= InitialLines || ++_quiet < QuietFrames)
        {
            return;
        }

        _quiet = 0;

        foreach (var sprite in sLeaser.sprites.Skip(size / 2))
        {
            sprite.RemoveFromContainer();
        }

        Array.Resize(ref sLeaser.sprites, size / 2);
    }

    private static void Scan(Room? room)
    {
        _pairs.Clear();

        if (room == null || !Config.Links.IsActive)
        {
            return;
        }

        _objects.Clear();
        _objects.AddRange(room.Objects);

        for (int i = 0; i < _objects.Count; ++i)
        {
            for (int j = i + 1; j < _objects.Count; ++j)
            {
                PhysicalObject first = _objects[i];
                PhysicalObject second = _objects[j];

                if (Config.Owners.IsActive && (first.ExternalOwner?.RealizedOwner == second || second.ExternalOwner?.RealizedOwner == first))
                {
                    continue;
                }

                FriendshipType type = first.abstractPhysicalObject.FriendshipWith(second.abstractPhysicalObject);

                if (type != FriendshipType.None)
                {
                    _pairs.Add((first, second, type));
                }
            }
        }

        _objects.Clear();
    }
}
