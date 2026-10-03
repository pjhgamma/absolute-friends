using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class TagOverlay : HudOverlay
{
    private const string TrackedSymbol = "FriendA";

    private const string UntrackedSymbol = "MonkA";

    private const string DeadSymbol = "SurvivorA";

    private const int ExplosiveSpearData = 1;

    private const int ElectricSpearData = 2;

    private const int HellSpearData = 3;

    private readonly List<SubjectTag> _tags = [];

    private readonly OverlayPositions _positions = new();

    private readonly List<AbstractPhysicalObject> _sources = [];

    private readonly Dictionary<AbstractPhysicalObject, (bool IsTracked, bool IsFriend, AbstractCreature? Owner)> _subjects = [];

    private readonly UpdateTimer _timer = new();

    private bool _isViewed;

    private static bool IsEnabled => Config.FriendName.IsActive || Config.FriendIcon.IsActive || Config.OwnerName.IsActive || Config.OwnerIcon.IsActive;

    private IEnumerable<AbstractPhysicalObject> Sources => room.TaggedObjects;

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

                    Refresh();
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

            Refresh();
        }
        catch (Exception exception)
        {
            this.Destroy(exception);
        }
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
                    if (!_positions.TryGet(abstractPhysicalObject, rCam, camPos, timeStacker, out Vector2 position, out bool isDistant) || !IsDrawn(abstractPhysicalObject, isDistant))
                    {
                        continue;
                    }

                    SubjectTag tag = TagAt(index++);

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

    private static void DrawFriend(SideTag tag, AbstractPhysicalObject abstractPhysicalObject, bool isTracked, Vector2 position, bool isDistant, ref int row)
    {
        if (Config.FriendName.IsActive)
        {
            string name = abstractPhysicalObject is AbstractCreature creature
                ? creature.FriendName
                : abstractPhysicalObject.DefaultName;

            tag.SetName(name, Palette.Primary, position, OverlayUtils.Row(row++));
        }

        if (!Config.FriendIcon.IsActive)
        {
            return;
        }

        float gap = isDistant ? 0f : OverlayUtils.Row(row++);

        if (abstractPhysicalObject is not AbstractCreature abstractCreature)
        {
            tag.SetIcon(GetSymbol(abstractPhysicalObject), Palette.Primary, position, gap);
        }
        else if (!isTracked)
        {
            tag.SetIcon(UntrackedSymbol, Palette.Primary, position, gap);
        }
        else if (abstractCreature.state is { dead: true })
        {
            tag.SetIcon(DeadSymbol, Palette.Primary, position, gap);
        }
        else if (!isDistant)
        {
            tag.SetIcon(TrackedSymbol, Palette.Primary, position, gap);
        }
        else
        {
            (string symbol, Color color) = abstractCreature.Icon;

            tag.SetIcon(symbol, color, position, gap);
        }
    }

    private static void DrawOwner(SideTag tag, AbstractCreature owner, Vector2 position, ref int row)
    {
        if (Config.OwnerName.IsActive)
        {
            tag.SetName(owner.DefaultName, Palette.Secondary, position, OverlayUtils.Row(row++));
        }

        if (Config.OwnerIcon.IsActive)
        {
            tag.SetIcon(GetOwnerSymbol(owner), Palette.Secondary, position, OverlayUtils.Row(row++));
        }
    }

    private static string GetOwnerSymbol(AbstractCreature owner)
    {
        AbstractPhysicalObject subject = owner is AbstractOwner abstractOwner ? abstractOwner.PhysicalObject.abstractPhysicalObject : owner;

        return subject is AbstractCreature abstractCreature
            ? CreatureSymbol.SpriteNameOfCreature(CreatureSymbol.SymbolDataFromCreature(abstractCreature))
            : GetSymbol(subject);
    }

    private static string GetSymbol(AbstractPhysicalObject abstractPhysicalObject)
    {
        int intData = abstractPhysicalObject is AbstractSpear abstractSpear
            ? GetSpearData(abstractSpear)
            : ItemSymbol.SymbolDataFromItem(abstractPhysicalObject)?.intData ?? 0;

        return ItemSymbol.SpriteNameForItem(abstractPhysicalObject.type, intData);
    }

    private static int GetSpearData(AbstractSpear abstractSpear)
    {
        if (ModManager.DLCShared && abstractSpear.hue != 0f)
        {
            return HellSpearData;
        }

        if (ModManager.DLCShared && abstractSpear.electric)
        {
            return ElectricSpearData;
        }

        return abstractSpear.explosive ? ExplosiveSpearData : 0;
    }

    private bool IsDrawn(AbstractPhysicalObject abstractPhysicalObject, bool isDistant)
    {
        return !isDistant || (_subjects.TryGetValue(abstractPhysicalObject, out var subject) && subject.IsTracked);
    }

    private void Draw(SubjectTag tag, AbstractPhysicalObject abstractPhysicalObject, Vector2 position, bool isDistant)
    {
        int row = 0;

        tag.Clear();

        if (!_subjects.TryGetValue(abstractPhysicalObject, out var subject))
        {
            return;
        }

        if (subject.IsFriend)
        {
            DrawFriend(tag.Friend, abstractPhysicalObject, subject.IsTracked, position, isDistant, ref row);
        }

        if (!isDistant && subject.Owner is { } owner)
        {
            DrawOwner(tag.Owner, owner, position, ref row);
        }
    }

    private void Refresh()
    {
        _subjects.Clear();

        foreach (var abstractPhysicalObject in _sources)
        {
            bool isTracked = abstractPhysicalObject.IsTracked;

            _subjects[abstractPhysicalObject] = (isTracked, isTracked || abstractPhysicalObject.IsFriendOfPlayer, abstractPhysicalObject.ExternalOwner);
        }
    }

    private SubjectTag TagAt(int index)
    {
        while (_tags.Count <= index)
        {
            SubjectTag tag = new();

            AddNodes(tag.Nodes);

            tag.IsVisible = false;

            _tags.Add(tag);
        }

        return _tags[index];
    }
}
