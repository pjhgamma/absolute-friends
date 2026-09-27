using System.Runtime.CompilerServices;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Core.Visualizer;

internal class FriendLinkOverlay : CosmeticSprite, IOverlay
{
    private const int InitialLines = 16;

    private const int SpareFactor = 4;

    private const float LineWidth = 1.5f;

    private const int QuietFrames = RainWorldUtils.Second;

    private static readonly HashSet<Pair> _pairs = [];

    private static RoomCamera? _camera;

    private int _quiet;

    public static void Refresh(RainWorldGame? game)
    {
        _camera = RainWorldUtils.MainCamera(game);

        _pairs.RemoveWhere(pair => !pair.IsCurrentFriend(_camera?.room));
    }

    public static void Track(UpdatableAndDeletable? source, UpdatableAndDeletable? target)
    {
        if (
            !Config.FriendLink.IsActive
            || source == null
            || target == null
            || ReferenceEquals(source, target)
            || _camera is not { room: { } viewed } camera
            || source.room != viewed
            || target.room != viewed
        )
        {
            return;
        }

        Pair pair = new(source, target);

        if (_pairs.Contains(pair) || !OverlayUtils.TryGetLine(camera, source, target, 1f, out _, out _))
        {
            return;
        }

        _pairs.Add(pair);
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
            if (OverlayUtils.ShouldDraw(rCam, room))
            {
                Fit(sLeaser, rCam, _pairs.Count);

                foreach (var link in _pairs)
                {
                    if (
                        link.First.room != room
                        || link.Second.room != room
                        || !OverlayUtils.TryGetLine(rCam, link.First, link.Second, timeStacker, out Vector2 first, out Vector2 second)
                    )
                    {
                        continue;
                    }

                    FSprite sprite = sLeaser.sprites[drawnCount++];

                    sprite.isVisible = true;
                    sprite.color = Palette.Primary;
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

    private readonly struct Pair(UpdatableAndDeletable first, UpdatableAndDeletable second) : IEquatable<Pair>
    {
        public UpdatableAndDeletable First { get; } = first;

        public UpdatableAndDeletable Second { get; } = second;

        public bool IsCurrentFriend(Room? room)
        {
            return room != null
                && First.room == room
                && Second.room == room
                && (First as PhysicalObject).IsFriend(Second as PhysicalObject);
        }

        public bool Equals(Pair other)
        {
            return (ReferenceEquals(First, other.First) && ReferenceEquals(Second, other.Second))
                || (ReferenceEquals(First, other.Second) && ReferenceEquals(Second, other.First));
        }

        public override bool Equals(object? obj) => obj is Pair other && Equals(other);

        public override int GetHashCode() => RuntimeHelpers.GetHashCode(First) ^ RuntimeHelpers.GetHashCode(Second);
    }
}
