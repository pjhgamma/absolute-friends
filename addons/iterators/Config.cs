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

    public static void Bind(Addon addon)
    {
        Moon = addon.Bind("Moon", false, new ConfigurableInfo("Treats Moon as a friend and prevents players from lowering her opinion or making her refuse to speak.", tags: ["Moon"]));
        MoonNeuron = addon.Bind("MoonNeuron", false, new ConfigurableInfo("Treats Moon's neurons as friends and prevents players from stealing them.", tags: ["Moon Neuron"]));
        Pebbles = addon.Bind("Pebbles", false, new ConfigurableInfo("Treats Pebbles as a friend and prevents him from killing friends.", tags: ["Pebbles"]));
        PebblesPearl = addon.Bind("PebblesPearl", false, new ConfigurableInfo("Treats Pebbles' pearl as friends and prevents players from stealing them.", tags: ["Pebbles Pearl"]));
        Prince = addon.Bind("Prince", false, new ConfigurableInfo("Treats the Prince as a friend of players.", tags: ["Prince"]));
        Overseer = addon.Bind("Overseer", false, new ConfigurableInfo("Always treats overseers as friends of players.", tags: ["Overseers"]));
        Mark = addon.Bind("Mark", false, new ConfigurableInfo("Tracked friends also show the mark while their player's mark is visible.", tags: ["Friend Mark"]));
    }
}
