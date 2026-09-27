using AbsoluteFriends.Options;

namespace AbsoluteFriends.Progression;

internal static class Config
{
    public static Configurable<bool> Gate = null!;

    public static Configurable<float> GateTime = null!;

    public static Configurable<bool> GateForce = null!;

    public static Configurable<float> GateForceTime = null!;

    public static Configurable<bool> Passage = null!;

    public static Configurable<bool> TempleGuard = null!;

    public static Configurable<bool> ResonanceGate = null!;

    public static Configurable<bool> ResonanceRoom = null!;

    public static Configurable<bool> ResonanceGrab = null!;

    public static Configurable<bool> ResonanceWarp = null!;

    public static Configurable<bool> ResonanceMend = null!;

    public static Configurable<bool> ResonanceCost = null!;

    public static Configurable<float> ResonanceCostRatio = null!;

    public static Configurable<bool> ResonanceAftershock = null!;

    public static Configurable<float> ResonanceAftershockRatio = null!;

    public static Configurable<bool> ResonanceEffect = null!;

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
        Passage = addon.Bind("Passage", false, new ConfigurableInfo("Tracked friends that survive the cycle travel with the player from anywhere in the world, even through a passage.", tags: ["Passages"]));
        TempleGuard = addon.Bind("TempleGuard", false, new ConfigurableInfo("Guardians do not target tracked friends or move them with telekinesis when the players meet the Karma requirement to pass.", tags: ["Guardian"]));

        ResonanceGate = addon.Bind("ResonanceGate", false, new ConfigurableInfo(Options.Config.Watcher(
            "Resonance occurs automatically when a shelter or karma gate closes, or a karma room activates, reaching tracked friends anywhere in the world.",
            "Resonance occurs automatically when a shelter or karma gate closes, or a karma room or warp point activates, reaching tracked friends anywhere in the world."
        ), tags: ["Gate Resonance"]));
        ResonanceRoom = addon.Bind("ResonanceRoom", false, new ConfigurableInfo("Resonance occurs when a player presses jump, reaching friends in the room. With Gate Resonance enabled, resonance inside a gate also reaches tracked friends in other rooms.", tags: ["Room Resonance"]));
        ResonanceGrab = addon.Bind("ResonanceGrab", false, new ConfigurableInfo("Resonance occurs when a player holds a tracked friend and presses jump, mending or reviving that friend.", tags: ["Grab Resonance"]));
        ResonanceWarp = addon.Bind("ResonanceWarp", false, new ConfigurableInfo("Warps eligible tracked friends to the player's location upon resonance.", tags: ["Warp"]))
            .RequireAny(ResonanceGate, ResonanceRoom);
        ResonanceMend = addon.Bind("ResonanceMend", false, new ConfigurableInfo("Mends eligible tracked friends upon resonance, closing their wounds and drawing out poison and cold, and bringing back the ones that have died.", tags: ["Mend"]))
            .RequireAny(ResonanceGate, ResonanceRoom, ResonanceGrab);
        ResonanceCost = addon.Bind("ResonanceCost", true, new ConfigurableInfo("A resonance demands a cost, which grows with the distance each friend is called from and the time it has spent dead, and holds everyone who resonated still and out of breath.", tags: ["Cost"]))
            .RequireAny(ResonanceWarp, ResonanceMend);
        ResonanceCostRatio = addon.Bind("ResonanceCostRatio", 1f, new ConfigurableInfo("Sets how much of the resonance cost is demanded. A value of 1 demands it in full, and 0 makes a resonance instant and free."))
            .Require(ResonanceCost);
        ResonanceAftershock = addon.Bind("ResonanceAftershock", true, new ConfigurableInfo("The paid resonance cost lingers as an aftershock, making everyone who resonated stumble now and then until the cycle ends.", tags: ["Aftershock"]))
            .Require(ResonanceCost);
        ResonanceAftershockRatio = addon.Bind("ResonanceAftershockRatio", 0.1f, new ConfigurableInfo("Sets how much of the paid resonance cost lingers as an aftershock. A value of 1 leaves all of it behind, and 0 leaves nothing."))
            .Require(ResonanceAftershock);
        ResonanceEffect = addon.Bind("ResonanceEffect", true, new ConfigurableInfo("Displays visual effects upon resonance. A gate resonance melts the room into the void, while a room or grab resonance answers with ripples.", tags: ["Visual Effects"]))
            .RequireAny(ResonanceWarp, ResonanceMend);
    }
}
