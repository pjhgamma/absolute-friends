using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using System.Reflection;

namespace RippleFriends.Options;

internal sealed class Preset(string name, string description, bool baseline, params Configurable<bool>[] deviations)
{
    private static FieldInfo[]? _fields;

    private static Preset[]? _presets;

    private static Configurable<bool>[]? _diagnostics;

    public static Preset[] Presets => _presets ??= [
        new("STILL WATER", "No one is a Ripple Friend, and the world runs as it always has.", false),
        new(
            "POINT",
            "Only slugcats are Ripple Friends, and only their own bodies and some of the objects they throw turn aside from them.",
            false,
            Config.FriendSlugcat,
            Config.Collision,
            Config.Rock,
            Config.Spear,
            Config.ExplosiveSpear,
            Config.ElectricSpear,
            Config.HellSpear,
            Config.PoisonSpear,
            Config.LilyPuck,
            Config.Boomerang,
            Config.WaterNut,
            Config.GrabbingPlayer,
            Config.Wiggle,
            Config.Carry,
            Config.Mauling,
            Config.GourmandSlam,
            Config.ArtificerParry,
            Config.SaintTongue,
            Config.SaintAttunement,
            Config.WatcherRipple
        ),
        new(
            "LOOP",
            "Only slugcats are Ripple Friends, yet every phenomenon turns aside from them, and the world waits at its doors. These are the mod's defaults.",
            true,
            Config.FriendCreature,
            Config.FriendNeutralCreature,
            Config.FriendIterator,
            Config.FriendChaining,
            Config.FriendArena,
            Config.Violence,
            Config.LizardBite,
            Config.LizardTongue,
            Config.LizardSpit,
            Config.LizardBeam,
            Config.LizardBlizzard,
            Config.LizardPoison,
            Config.ScavengerShelter,
            Config.ScavengerTemplar,
            Config.Moon,
            Config.MoonNeuron,
            Config.Pebbles,
            Config.PebblesPearl,
            Config.ResonanceGate,
            Config.ResonanceRoom,
            Config.ResonanceGrab,
            Config.ResonanceWarp,
            Config.ResonanceMend,
            Config.ResonanceEffect
        ),
        new("SPIRAL", "Friendly creatures and Iterators are drawn in as well, and resonance answers when it is called.", true, Config.FriendNeutralCreature, Config.FriendChaining, Config.FriendArena, Config.Violence),
        new("RIPPLE", "Every creature that could be a Ripple Friend becomes one, and nothing that could reach them does.", true)
    ];

    public static Preset? Current => Presets.FirstOrDefault(preset => preset.IsApplied);

    public string Name => name;

    public string Description => description;

    private static Configurable<bool>[] Diagnostics => _diagnostics ??= [
        Config.FriendLink,
        Config.FriendName,
        Config.FriendIcon,
        Config.OwnerLink,
        Config.OwnerName,
        Config.OwnerIcon,
        Config.Debug
    ];

    private static IEnumerable<(Configurable<bool> configurable, OpCheckBox checkBox)> CheckBoxes
    {
        get
        {
            foreach (var fieldInfo in _fields ??= typeof(Config).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (fieldInfo.GetValue(null) is Configurable<bool> configurable && !Diagnostics.Contains(configurable) && configurable.BoundUIconfig is OpCheckBox checkBox)
                {
                    yield return (configurable, checkBox);
                }
            }
        }
    }

    private bool IsApplied
    {
        get
        {
            foreach (var (configurable, checkBox) in CheckBoxes)
            {
                if (checkBox.GetValueBool() != ValueOf(configurable))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static void Watch(Action action)
    {
        foreach (var (_, checkBox) in CheckBoxes)
        {
            checkBox.OnChange += delegate
            {
                action();
            };
        }
    }

    public void Apply()
    {
        foreach (var (configurable, checkBox) in CheckBoxes)
        {
            checkBox.SetValueBool(ValueOf(configurable));
        }
    }

    private bool ValueOf(Configurable<bool> configurable)
    {
        return deviations.Contains(configurable) != baseline;
    }
}
