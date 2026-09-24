using AbsoluteFriends.Options;

namespace AbsoluteFriends.Iterators;

internal static class Config
{
    public static Configurable<bool> Moon = null!;

    public static Configurable<bool> MoonNeuron = null!;

    public static Configurable<bool> Pebbles = null!;

    public static Configurable<bool> PebblesPearl = null!;

    public static void Bind(Addon addon)
    {
        Moon = addon.Bind("Moon", false, new ConfigurableInfo("Prevents players from lowering Moon's opinion or making her refuse to speak.", tags: ["Moon"]))
            .Require(Core.Config.FriendSlugcat);
        MoonNeuron = addon.Bind("MoonNeuron", false, new ConfigurableInfo("Prevents players from stealing Moon's neurons.", tags: ["Moon Neuron"]))
            .Require(Core.Config.FriendSlugcat);
        Pebbles = addon.Bind("Pebbles", false, new ConfigurableInfo("Prevents Pebbles from killing friends.", tags: ["Pebbles"]))
            .Require(Core.Config.FriendSlugcat);
        PebblesPearl = addon.Bind("PebblesPearl", false, new ConfigurableInfo("Prevents players from stealing Pebbles' pearl.", tags: ["Pebbles Pearl"]))
            .Require(Core.Config.FriendSlugcat);
    }
}
