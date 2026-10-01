using AbsoluteFriends.Options;

namespace AbsoluteFriends.Resonance;

internal static class Config
{
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
        ResonanceGate = addon.Bind("ResonanceGate", true, new ConfigurableInfo(Options.Config.Watcher(
            "Resonance occurs automatically when a shelter or karma gate closes, or a karma room activates, reaching tracked friends anywhere in the world.",
            "Resonance occurs automatically when a shelter or karma gate closes, or a karma room or warp point activates, reaching tracked friends anywhere in the world."
        ), tags: ["Gate Resonance"]));
        ResonanceRoom = addon.Bind("ResonanceRoom", true, new ConfigurableInfo("Resonance occurs when a player presses jump, reaching friends in the room. With Gate Resonance enabled, resonance inside a gate also reaches tracked friends in other rooms.", tags: ["Room Resonance"]));
        ResonanceGrab = addon.Bind("ResonanceGrab", true, new ConfigurableInfo("Resonance occurs when a player holds a tracked friend and presses jump, mending or reviving that friend.", tags: ["Grab Resonance"]));
        ResonanceWarp = addon.Bind("ResonanceWarp", true, new ConfigurableInfo("Warps eligible tracked friends to the player's location upon resonance. Warped friends stay where they arrived after disabling this option.", tags: ["Warp"]))
            .RequireAny(ResonanceGate, ResonanceRoom);
        ResonanceMend = addon.Bind("ResonanceMend", true, new ConfigurableInfo("Mends eligible tracked friends upon resonance, closing their wounds and drawing out poison and cold, and bringing back the ones that have died. Revived friends stay alive after disabling this option.", tags: ["Mend"]))
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
