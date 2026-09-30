using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class TagOverlay : TrackerOverlay<SubjectTag>
{
    private const string TrackedSymbol = "FriendA";

    private const string UntrackedSymbol = "MonkA";

    private const string DeadSymbol = "SurvivorA";

    private const int ExplosiveSpearData = 1;

    private const int ElectricSpearData = 2;

    private const int HellSpearData = 3;

    private readonly Dictionary<AbstractPhysicalObject, (bool IsTracked, bool IsFriend, AbstractCreature? Owner)> _subjects = [];

    protected override bool IsEnabled => Config.FriendName.IsActive || Config.FriendIcon.IsActive || Config.OwnerName.IsActive || Config.OwnerIcon.IsActive;

    protected override IEnumerable<AbstractPhysicalObject> Sources => room.TaggedObjects;

    protected override SubjectTag CreateTag() => new();

    protected override bool IsDrawn(AbstractPhysicalObject abstractPhysicalObject, bool isDistant)
    {
        return !isDistant || (_subjects.TryGetValue(abstractPhysicalObject, out var subject) && subject.IsTracked);
    }

    protected override void Draw(SubjectTag tag, AbstractPhysicalObject abstractPhysicalObject, Vector2 position, bool isDistant)
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

    protected override void Refresh(IReadOnlyList<AbstractPhysicalObject> sources)
    {
        _subjects.Clear();

        foreach (var abstractPhysicalObject in sources)
        {
            bool isTracked = abstractPhysicalObject.IsTracked;

            _subjects[abstractPhysicalObject] = (isTracked, isTracked || abstractPhysicalObject.IsFriendOfPlayer, abstractPhysicalObject.ExternalOwner);
        }
    }

    private static void DrawFriend(SideTag tag, AbstractPhysicalObject abstractPhysicalObject, bool isTracked, Vector2 position, bool isDistant, ref int row)
    {
        if (Config.FriendName.IsActive)
        {
            string name = abstractPhysicalObject is AbstractCreature creature
                ? creature.FriendName
                : abstractPhysicalObject.DefaultName;

            tag.SetName(name, position, OverlayUtils.Row(row++));
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
            tag.SetName(owner.DefaultName, position, OverlayUtils.Row(row++));
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
}
