using System.Runtime.CompilerServices;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal abstract class TrackerOverlay<TTag> : CosmeticSprite, IOverlay
    where TTag : ITag
{
    private readonly FContainer _container = new();

    private readonly List<TTag> _tags = [];

    private readonly ConditionalWeakTable<AbstractPhysicalObject, StrongBox<Vector2>> _lastPlaces = new();

    private readonly List<AbstractPhysicalObject> _sources = [];

    private readonly UpdateTimer _timer = new();

    private bool _isViewed;

    protected abstract bool IsEnabled { get; }

    protected abstract IEnumerable<AbstractPhysicalObject> Sources { get; }

    public override void Update(bool eu)
    {
        base.Update(eu);

        try
        {
            if (RainWorldUtils.MainCamera(room?.game)?.room != room)
            {
                if (_isViewed)
                {
                    _isViewed = false;
                    _sources.Clear();

                    Refresh(_sources);
                }

                return;
            }

            if (!_isViewed)
            {
                _isViewed = true;

                _timer.Reset();
            }

            if (!_timer.Elapse())
            {
                return;
            }

            _sources.Clear();

            if (IsEnabled)
            {
                _sources.AddRange(Sources);
            }

            Refresh(_sources);
        }
        catch (Exception exception)
        {
            this.Destroy(exception);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [];
        sLeaser.containers = [_container];

        AddToContainer(sLeaser, rCam, rCam.HudContainer);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        (newContatiner ?? rCam.HudContainer).AddChild(_container);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        try
        {
            int index = 0;

            if (IsEnabled && OverlayUtils.ShouldDraw(rCam, room))
            {
                foreach (var abstractPhysicalObject in _sources)
                {
                    if (!TryGetScreenPosition(abstractPhysicalObject, rCam, camPos, timeStacker, out Vector2 position, out bool isDistant) || !IsDrawn(abstractPhysicalObject, isDistant))
                    {
                        continue;
                    }

                    TTag tag = TagAt(index++);

                    Draw(tag, abstractPhysicalObject, position, isDistant);

                    tag.IsDistant = isDistant;
                    tag.IsVisible = true;
                }
            }

            for (int i = index; i < _tags.Count; ++i)
            {
                _tags[i].IsVisible = false;
            }
        }
        catch (Exception exception)
        {
            this.Destroy(exception);
        }
    }

    protected abstract TTag CreateTag();

    protected abstract void Draw(TTag tag, AbstractPhysicalObject abstractPhysicalObject, Vector2 position, bool isDistant);

    protected virtual bool IsDrawn(AbstractPhysicalObject abstractPhysicalObject, bool isDistant) => true;

    protected virtual void Refresh(IReadOnlyList<AbstractPhysicalObject> sources)
    {
    }

    private static bool TryGetWorldPosition(AbstractPhysicalObject abstractPhysicalObject, World world, float timeStacker, out Vector2 position)
    {
        if (
            abstractPhysicalObject.realizedObject is { room.abstractRoom: { } current } realizedObject
            && world.GetAbstractRoom(current.index) != null
            && realizedObject.TryGetPosition(timeStacker, out Vector2 inRoom)
        )
        {
            position = world.RoomToWorldPos(inRoom, current.index);

            return true;
        }

        if (world.GetAbstractRoom(abstractPhysicalObject.pos.room) is { } abstractRoom)
        {
            position = world.RoomToWorldPos(abstractPhysicalObject.pos.PositionInRoom(abstractRoom), abstractRoom.index);

            return true;
        }

        position = default;

        return false;
    }

    private TTag TagAt(int index)
    {
        while (_tags.Count <= index)
        {
            TTag tag = CreateTag();

            foreach (var node in tag.Nodes)
            {
                _container.AddChild(node);
            }

            tag.IsVisible = false;

            _tags.Add(tag);
        }

        return _tags[index];
    }

    private bool TryGetScreenPosition(AbstractPhysicalObject abstractPhysicalObject, RoomCamera camera, Vector2 camPos, float timeStacker, out Vector2 position, out bool isDistant)
    {
        position = default;
        isDistant = true;

        if (abstractPhysicalObject.slatedForDeletion || camera.room?.world is not { } world || camera.room.abstractRoom is not { } viewed)
        {
            return false;
        }

        if (TryGetWorldPosition(abstractPhysicalObject, world, timeStacker, out Vector2 inWorld))
        {
            _lastPlaces.GetOrCreateValue(abstractPhysicalObject).Value = inWorld;

            isDistant = abstractPhysicalObject.realizedObject?.room != camera.room;
        }
        else if (_lastPlaces.TryGetValue(abstractPhysicalObject, out StrongBox<Vector2> last))
        {
            inWorld = last.Value;
        }
        else
        {
            return false;
        }

        Vector2 onScreen = inWorld - world.RoomToWorldPos(camPos, viewed.index);

        position = onScreen.ClampToScreen(camera);
        isDistant |= position != onScreen;

        return true;
    }
}
