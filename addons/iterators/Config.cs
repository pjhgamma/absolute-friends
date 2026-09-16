using RippleFriends.Options;

namespace RippleFriends.Iterators;

internal static class Config
{
    public static Configurable<bool> Moon = null!;

    public static Configurable<bool> MoonNeuron = null!;

    public static Configurable<bool> Pebbles = null!;

    public static Configurable<bool> PebblesPearl = null!;

    public static void Bind(Addon addon)
    {
        Moon = addon.Bind("Moon", false, new ConfigurableInfo("Will not lower the opinion of Moon Ripple Friend, nor make her refuse to speak.", tags: ["Moon"]))
            .Require(Core.Config.FriendSlugcat);
        MoonNeuron = addon.Bind("MoonNeuron", false, new ConfigurableInfo("Will not steal the Neurons of Moon Ripple Friend.", tags: ["Moon Neuron"]))
            .Require(Core.Config.FriendSlugcat);
        Pebbles = addon.Bind("Pebbles", false, new ConfigurableInfo("Will not be killed by Pebbles Ripple Friend.", tags: ["Pebbles"]))
            .Require(Core.Config.FriendSlugcat);
        PebblesPearl = addon.Bind("PebblesPearl", false, new ConfigurableInfo("Will not steal the pearl of Pebbles Ripple Friend.", tags: ["Pebbles Pearl"]))
            .Require(Core.Config.FriendSlugcat);
    }
}
