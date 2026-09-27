using System.Runtime.CompilerServices;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Core.Visualizer;

internal interface IOverlay;

internal interface ITag
{
    IEnumerable<FNode> Nodes { get; }

    bool IsVisible { set; }

    bool IsDistant { set; }
}

internal static class OverlayUtils
{
    public const float Margin = 20f;

    public const float Gap = 20f;

    private static readonly AddonLogger _logger = Reporter.GetLogger(Plugin.Name);

    private static readonly HashSet<Type> _failures = [];

    private static RainWorldGame? _game;

    public static float Row(int index) => Gap * (index + 1);

    public static bool ShouldDraw(RoomCamera? camera, Room? room) => camera != null && camera.room == room && camera == RainWorldUtils.MainCamera(room?.game);

    public static void Attach<TSource, TOverlay>(ConditionalWeakTable<TSource, TOverlay> overlays, TSource source, Room room, Func<TSource, TOverlay> create)
        where TSource : class
        where TOverlay : UpdatableAndDeletable
    {
        if (!ReferenceEquals(_game, RainWorldUtils.CurrentGame))
        {
            _game = RainWorldUtils.CurrentGame;

            _failures.Clear();
        }

        if (_failures.Contains(typeof(TOverlay)))
        {
            return;
        }

        if (overlays.TryGetValue(source, out TOverlay overlay))
        {
            if (!overlay.slatedForDeletetion)
            {
                return;
            }

            overlays.Remove(source);
        }

        overlay = create(source);

        overlays.Add(source, overlay);

        room.AddObject(overlay);
    }

    public static bool TryGetLine(RoomCamera camera, UpdatableAndDeletable? start, UpdatableAndDeletable? end, float timeStacker, out Vector2 first, out Vector2 second)
    {
        if (
            start.TryGetPosition(timeStacker, out first)
            && end.TryGetPosition(timeStacker, out second)
            && first.IsOnScreen(camera)
            && second.IsOnScreen(camera)
        )
        {
            return true;
        }

        first = second = default;

        return false;
    }

    private static Vector2 Interpolate(Vector2 last, Vector2 now, float timeStacker) => Vector2.Lerp(last, now, timeStacker);

    private static Vector2 Interpolate(BodyChunk chunk, float timeStacker) => Interpolate(chunk.lastPos, chunk.pos, timeStacker);

    extension(RoomCamera camera)
    {
        public FContainer HudContainer => camera.ReturnFContainer("HUD");
    }

    extension(Room room)
    {
        public IEnumerable<AbstractPhysicalObject> TaggedObjects
        {
            get
            {
                foreach (var abstractCreature in FriendUtils.TrackedFriends)
                {
                    yield return abstractCreature;
                }

                foreach (var physicalObject in room.Objects)
                {
                    if (!physicalObject.IsTracked && (physicalObject.Owner != null || (physicalObject is Creature && physicalObject.IsFriendOfPlayer)))
                    {
                        yield return physicalObject.abstractPhysicalObject;
                    }
                }
            }
        }
    }

    extension(UpdatableAndDeletable? source)
    {
        public bool TryGetPosition(float timeStacker, out Vector2 position)
        {
            Vector2? found = source switch
            {
                Creature { mainBodyChunk: { } main } => Interpolate(main, timeStacker),
                PhysicalObject { firstChunk: { } first } => Interpolate(first, timeStacker),
                Explosion explosion => explosion.pos,
                SporePlant.Bee bee => Interpolate(bee.lastPos, bee.pos, timeStacker),
                _ => null,
            };

            position = found ?? default;

            return found.HasValue;
        }
    }

    extension(UpdatableAndDeletable overlay)
    {
        public void Destroy(Exception exception)
        {
            overlay.Destroy();

            Type type = overlay.GetType();

            _failures.Add(type);

            _logger.LogError($"{type.Name} threw an exception and was disabled", exception);
        }
    }

    extension(Vector2 position)
    {
        public bool IsOnScreen(RoomCamera camera) => camera.PositionCurrentlyVisible(position, Margin, false);

        public Vector2 ClampToScreen(RoomCamera camera)
        {
            Vector2 size = camera.sSize;

            return new(
                Mathf.Clamp(position.x, Margin, size.x - Margin),
                Mathf.Clamp(position.y, Margin, size.y - Margin)
            );
        }
    }

    extension(WorldCoordinate coordinate)
    {
        public Vector2 PositionInRoom(AbstractRoom abstractRoom)
        {
            IntVector2? tile = coordinate switch
            {
                { TileDefined: true } => coordinate.Tile,
                { NodeDefined: true } when abstractRoom.realizedRoom?.exitAndDenIndex is { } exits && coordinate.abstractNode < exits.Length => exits[coordinate.abstractNode],
                _ => null,
            };

            return tile is { } found ? new Vector2(found.x + 0.5f, found.y + 0.5f) * 20f : new Vector2(abstractRoom.size.x, abstractRoom.size.y) * 20f / 2f;
        }
    }

    extension(AbstractPhysicalObject? abstractPhysicalObject)
    {
        public string DisplayName => abstractPhysicalObject switch
        {
            AbstractOwner abstractOwner => abstractOwner.DisplayName,
            AbstractCreature { creatureTemplate.name: { } name } abstractCreature => $"{name} {abstractCreature.ID.number}",
            { type: { } type } => $"{type} {abstractPhysicalObject.ID.number}",
            _ => "",
        };
    }

    extension(FFacetNode node)
    {
        public bool IsHologram
        {
            set
            {
                try
                {
                    node.shader = (value ? RainWorldUtils.Shader("Hologram") : null) ?? FShader.defaultShader;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning("Could not reach the hologram shader", exception);
                }
            }
        }
    }
}
