using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal enum ShownFriendshipChoice
{
    Tracked,
    Friendly,
    Neutral,
    Indirect,
}

internal enum RelationshipIconStyle
{
    Creature,
    Symbol,
}

internal readonly struct RelationshipAppearance(string name, string symbol, Color color)
{
    public string Name { get; } = name;

    public string Symbol { get; } = symbol;

    public Color Color { get; } = color;
}

internal static class Relationships
{
    private const int ExplosiveSpearData = 1;

    private const int ElectricSpearData = 2;

    private const int HellSpearData = 3;

    public static FriendshipType WeakestShown => (ShownFriendshipChoice)Config.ShownFriendship.Value switch
    {
        ShownFriendshipChoice.Tracked => FriendshipType.Tracked,
        ShownFriendshipChoice.Friendly => FriendshipType.Friendly,
        ShownFriendshipChoice.Neutral => FriendshipType.Neutral,
        _ => FriendshipType.Indirect,
    };

    public static bool IsShown(FriendshipType type) => Config.FriendRelationship.IsActive && type != FriendshipType.None && type >= WeakestShown;

    public static Color ColorOf(FriendshipType type) => type switch
    {
        FriendshipType.Tracked => Palette.Tracked,
        FriendshipType.Friendly => Palette.Friendly,
        FriendshipType.Neutral => Palette.Neutral,
        FriendshipType.Indirect => Palette.Indirect,
        _ => Palette.Unfriendly,
    };

    public static RelationshipAppearance AppearanceOf(AbstractPhysicalObject source, FriendshipType type, bool isOwner)
    {
        AbstractPhysicalObject subject = source is AbstractOwner owner ? owner.PhysicalObject.abstractPhysicalObject : source;
        string name = string.Empty;
        string symbol = string.Empty;
        Color color = isOwner ? Palette.Owned : ColorOf(type);

        if (Config.Name.IsActive)
        {
            name = source is AbstractCreature creature ? creature.FriendName : source.DefaultName;
        }

        if (Config.Icon.IsActive)
        {
            symbol = subject is AbstractCreature abstractCreature ? CreatureSymbolOf(abstractCreature, type) : ItemSymbolOf(subject);
        }

        if (subject is AbstractCreature { IsDead: true })
        {
            color = color.Dimmed;
        }

        return new(name, symbol, color);
    }

    private static string CreatureSymbolOf(AbstractCreature creature, FriendshipType type)
    {
        if (Config.IconStyle.Value != (int)RelationshipIconStyle.Symbol)
        {
            return creature.Icon.Symbol;
        }

        return type switch
        {
            FriendshipType.Tracked => "FriendA",
            FriendshipType.Friendly => "SaintA",
            FriendshipType.Neutral => "MonkA",
            FriendshipType.Indirect => "TravellerA",
            _ => "OutlawA",
        };
    }

    private static string ItemSymbolOf(AbstractPhysicalObject source)
    {
        int data = source is AbstractSpear spear ? SpearData(spear) : ItemSymbol.SymbolDataFromItem(source)?.intData ?? 0;

        return ItemSymbol.SpriteNameForItem(source.type, data);
    }

    private static int SpearData(AbstractSpear spear)
    {
        if (ModManager.DLCShared && spear.hue != 0f)
        {
            return HellSpearData;
        }

        if (ModManager.DLCShared && spear.electric)
        {
            return ElectricSpearData;
        }

        return spear.explosive ? ExplosiveSpearData : 0;
    }
}
