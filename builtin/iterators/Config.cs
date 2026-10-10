namespace AbsoluteFriends.Iterators;

internal static class Config
{
    public static Configurable<bool> Moon = null!;

    public static Configurable<bool> MoonNeuron = null!;

    public static Configurable<bool> Pebbles = null!;

    public static Configurable<bool> PebblesPearl = null!;

    public static Configurable<bool> Prince = null!;

    public static Configurable<bool> Overseer = null!;

    public static Configurable<bool> Mark = null!;

    public static Configurable<bool> Glow = null!;

    public static void Bind(Addon addon)
    {
        Moon = addon.Bind("Moon", true, new ConfigurableInfo("Treats Moon as a friend and prevents players from lowering her opinion or making her refuse to speak. Her preserved opinion remains after disabling this option.", tags: ["Moon"]));
        MoonNeuron = addon.Bind("MoonNeuron", true, new ConfigurableInfo("Treats Moon's neurons as friends and prevents players from stealing them.", tags: ["Moon Neuron"]));
        Pebbles = addon.Bind("Pebbles", true, new ConfigurableInfo("Treats Pebbles as a friend and prevents him from killing friends.", tags: ["Pebbles"]));
        PebblesPearl = addon.Bind("PebblesPearl", true, new ConfigurableInfo("Treats Pebbles' pearl as friends and prevents players from stealing them.", tags: ["Pebbles Pearl"]));
        Prince = addon.Bind("Prince", true, new ConfigurableInfo("Treats the Prince as a friend.", tags: ["Prince"]));
        Overseer = addon.Bind("Overseer", true, new ConfigurableInfo("Treats overseers as friends.", tags: ["Overseers"]));
        Mark = addon.Bind("Mark", true, new ConfigurableInfo("Tracked friends also show the mark while their player's mark is visible.", tags: ["Mark"]));
        Glow = addon.Bind("Glow", true, new ConfigurableInfo("Tracked friends also glow while their player glows.", tags: ["Glow"]));

        addon.Group("Moon", "Treats Moon and her neurons as friends.", Moon, MoonNeuron);
        addon.Group("Pebbles", Options.Config.Downpour("Treats Pebbles as a friend.", "Treats Pebbles and his pearls as friends."), Pebbles, PebblesPearl);
        addon.Group("Prince", "Treats the Prince as a friend.", Prince);
        addon.Group("Overseers", "Treats overseers as friends.", Overseer);
        addon.Group("Enlightenment", "Tracked friends also show the mark and glow of their player.", Mark, Glow);
    }
}
