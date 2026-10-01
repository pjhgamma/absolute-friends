using AbsoluteFriends.Options;

namespace AbsoluteFriends.Pilgrimage;

internal static class Config
{
    public static Configurable<bool> Gate = null!;

    public static Configurable<float> GateTime = null!;

    public static Configurable<bool> GateForce = null!;

    public static Configurable<float> GateForceTime = null!;

    public static Configurable<bool> TempleGuard = null!;

    public static Configurable<bool> Ascension = null!;

    public static void Bind(Addon addon)
    {
        Gate = addon.Bind("Gate", true, new ConfigurableInfo(Options.Config.Watcher(
            "Shelters and karma gates wait for tracked friends and will not activate while any player is entering control inputs.",
            "Shelters, karma gates, and warp points wait for tracked friends and will not activate while any player is entering control inputs."
        ), tags: ["Gate Delay"]));
        GateTime = addon.Bind("GateTime", 1f, new ConfigurableInfo(Options.Config.Watcher(
            "Sets the minimum time (in seconds) of no control input from the player required for shelters and karma gates to activate.",
            "Sets the minimum time (in seconds) of no control input from the player required for shelters, karma gates, and warp points to activate."
        )))
            .Require(Gate);
        GateForce = addon.Bind("GateForce", true, new ConfigurableInfo(Options.Config.Watcher(
            "Shelters and karma gates forcefully activate, ignoring non-player friends.",
            "Shelters, karma gates, and warp points forcefully activate, ignoring non-player friends."
        ), tags: ["Gate Force"]))
            .Require(Gate);
        GateForceTime = addon.Bind("GateForceTime", 3f, new ConfigurableInfo(Options.Config.Watcher(
            "Sets how much longer (in seconds) the player must go without control input for shelters and karma gates to forcefully activate, ignoring non-player friends.",
            "Sets how much longer (in seconds) the player must go without control input for shelters, karma gates, and warp points to forcefully activate, ignoring non-player friends."
        )))
            .Require(GateForce);
        TempleGuard = addon.Bind("TempleGuard", true, new ConfigurableInfo("Guardians do not target tracked friends or move them with telekinesis when the players meet the Karma requirement to pass.", tags: ["Guardian"]));
        Ascension = addon.Bind("Ascension", true, new ConfigurableInfo(Options.Config.Watcher(
            "Tracked friends follow the player through the Void Sea and return to the last shelter after ascension.",
            "Tracked friends follow the player through the Void Sea and the final warps, and return to the last shelter after ascension."
        ), tags: ["Ascension"]));
    }
}
