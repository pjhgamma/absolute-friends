using System.Runtime.CompilerServices;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AbsoluteFriends.Core;

public static class CreatureIcon
{
    private static readonly AddonLogger _logger = Reporter.GetLogger(Plugin.Name);

    private static readonly ConditionalWeakTable<AbstractCreature, StrongBox<Color>> _shortcutColors = new();

    private static Color SlugpupColor(AbstractCreature creature)
    {
        if (creature.realizedCreature is Player player)
        {
            return player.ShortCutColor();
        }

        Color? story = creature.ID.RandomSeed switch
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

        Random.InitState(creature.ID.RandomSeed);

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

    extension(AbstractCreature creature)
    {
        public (string Symbol, Color Color) Icon
        {
            get
            {
                IconSymbol.IconSymbolData data = CreatureSymbol.SymbolDataFromCreature(creature);
                Color color = creature.IsSlugpup
                    ? SlugpupColor(creature)
                    : CreatureSymbol.ColorOfCreature(data);

                return (CreatureSymbol.SpriteNameOfCreature(data), color);
            }
        }

        public Color ShortcutColor
        {
            get
            {
                if (creature.realizedCreature is { } realized)
                {
                    try
                    {
                        Color color = realized.ShortCutColor();

                        _shortcutColors.GetOrCreateValue(creature).Value = color;

                        return color;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning("Could not read a shortcut color", exception);
                    }
                }

                return _shortcutColors.TryGetValue(creature, out var known) ? known.Value : creature.Icon.Color;
            }
        }
    }
}
