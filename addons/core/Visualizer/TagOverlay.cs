using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AbsoluteFriends.Core.Visualizer;

internal class TagOverlay : TrackerOverlay<SubjectTag>
{
    private const string TrackedSymbol = "FriendA";

    private const string UntrackedSymbol = "MonkA";

    private const string DeadSymbol = "SurvivorA";

    private const int ExplosiveSpearData = 1;

    private const int ElectricSpearData = 2;

    private const int HellSpearData = 3;

    protected override bool IsEnabled => Config.FriendName.IsActive || Config.FriendIcon.IsActive || Config.OwnerName.IsActive || Config.OwnerIcon.IsActive;

    protected override IEnumerable<AbstractPhysicalObject> Sources => room.TaggedObjects;

    protected override SubjectTag CreateTag() => new();

    protected override bool IsDrawn(AbstractPhysicalObject abstractPhysicalObject, bool isDistant)
    {
        return !isDistant || abstractPhysicalObject.IsTracked;
    }

    protected override void Draw(SubjectTag tag, AbstractPhysicalObject abstractPhysicalObject, Vector2 position, bool isDistant)
    {
        int row = 0;

        tag.Clear();

        if (abstractPhysicalObject.IsTracked || abstractPhysicalObject.IsFriendOfPlayer)
        {
            DrawFriend(tag.Friend, abstractPhysicalObject, position, isDistant, ref row);
        }

        if (!isDistant && abstractPhysicalObject.ExternalOwner is { } owner)
        {
            DrawOwner(tag.Owner, owner, position, ref row);
        }
    }

    private static void DrawFriend(SideTag tag, AbstractPhysicalObject abstractPhysicalObject, Vector2 position, bool isDistant, ref int row)
    {
        if (Config.FriendName.IsActive)
        {
            tag.SetName(abstractPhysicalObject.DisplayName, position, OverlayUtils.Row(row++));
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
        else if (!abstractCreature.IsTracked)
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
            IconSymbol.IconSymbolData iconData = CreatureSymbol.SymbolDataFromCreature(abstractCreature);

            tag.SetIcon(CreatureSymbol.SpriteNameOfCreature(iconData), GetColor(iconData, abstractCreature), position, gap);
        }
    }

    private static void DrawOwner(SideTag tag, AbstractCreature owner, Vector2 position, ref int row)
    {
        if (Config.OwnerName.IsActive)
        {
            tag.SetName(owner.DisplayName, position, OverlayUtils.Row(row++));
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

    private static Color GetColor(IconSymbol.IconSymbolData iconData, AbstractCreature abstractCreature)
    {
        return ModManager.MSC && abstractCreature.creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC
            ? GetSlugpupColor(abstractCreature)
            : CreatureSymbol.ColorOfCreature(iconData);
    }

    private static Color GetSlugpupColor(AbstractCreature abstractCreature)
    {
        if (abstractCreature.realizedCreature is Player player)
        {
            return player.ShortCutColor();
        }

        Color? story = abstractCreature.ID.RandomSeed switch
        {
            1000 => new Color(0.6f, 0.7f, 0.9f),
            1001 => new Color(0.48f, 0.87f, 0.81f),
            1002 => new Color(0.43922f, 0.13725f, 0.23529f),
            _ => null,
        };

        if (story is { } found)
        {
            return found;
        }

        Random.State state = Random.state;

        Random.InitState(abstractCreature.ID.RandomSeed);

        NextStat();
        float met = NextStat();
        float stealth = NextStat();
        NextStat();
        NextStat();

        float hue = Mathf.Lerp(Random.Range(0.15f, 0.58f), Random.value, Mathf.Pow(Random.value, 1.5f - met));
        float saturation = Mathf.Pow(Random.Range(0f, 1f), 0.3f + stealth * 0.3f);
        bool isDark = Random.Range(0f, 1f) <= 0.3f + stealth * 0.2f;
        float lightness = Mathf.Pow(Random.Range(isDark ? 0.9f : 0.75f, 1f), 1.5f - stealth);

        Random.state = state;

        return RWCustom.Custom.HSL2RGB(hue, saturation, Mathf.Clamp(isDark ? 1f - lightness : lightness, 0.01f, 1f));
    }

    private static float NextStat() => Mathf.Pow(Random.Range(0f, 1f), 1.5f);
}
