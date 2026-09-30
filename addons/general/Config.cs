using AbsoluteFriends.Options;

namespace AbsoluteFriends.General;

internal static class Config
{
    public static Configurable<bool> Collision = null!;

    public static Configurable<bool> Violence = null!;

    public static Configurable<bool> Explosion = null!;

    public static Configurable<bool> Fear = null!;

    public static Configurable<bool> Stealing = null!;

    public static Configurable<bool> Deaf = null!;

    public static Configurable<float> DeafRatio = null!;

    public static Configurable<bool> Blind = null!;

    public static Configurable<float> BlindRatio = null!;

    public static Configurable<bool> Hypothermia = null!;

    public static Configurable<float> HypothermiaRatio = null!;

    public static Configurable<bool> Breath = null!;

    public static Configurable<float> BreathRatio = null!;

    public static Configurable<bool> Forgiveness = null!;

    public static Configurable<float> ForgivenessRatio = null!;

    public static void Bind(Addon addon)
    {
        Collision = addon.Bind("Collision", true, new ConfigurableInfo("Will not collide with friends.", tags: ["Collisions"]));
        Violence = addon.Bind("Violence", false, new ConfigurableInfo("Friends take no damage from one another when the blow can be traced back to one of them.", tags: ["Violence"]));
        Explosion = addon.Bind("Explosion", false, new ConfigurableInfo("Will not be hit by most explosions caused by friends.", tags: ["Explosions"]));
        Fear = addon.Bind("Fear", false, new ConfigurableInfo("Will not be frightened by friends, nor by the weapons and explosives they wield.", tags: ["Fear"]));
        Stealing = addon.Bind("Stealing", false, new ConfigurableInfo(Options.Config.JollyCoop(
            "Friends cannot take objects held by other friends. Players can still take from slugpups.",
            "Friends cannot take objects held by other friends. Players can still take from slugpups. Jolly Co-op's No Stealing option always prevents theft between players."
        ), tags: ["Stealing"]));
        Deaf = addon.Bind("Deaf", true, new ConfigurableInfo("Deafness gradually fades toward that of the least deafened friend in the same room.", tags: ["Deaf"]));
        DeafRatio = addon.Bind("DeafRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in deafness fades away each frame while a less deafened friend is in the same room. A value of 1 closes the difference at once, and 0 disables the additional fading."))
            .Require(Deaf);
        Blind = addon.Bind("Blind", true, new ConfigurableInfo("Blindness gradually fades toward that of the least blinded friend in the same room.", tags: ["Blind"]));
        BlindRatio = addon.Bind("BlindRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in blindness fades away each frame while a less blinded friend is in the same room. A value of 1 closes the difference at once, and 0 disables the additional fading."))
            .Require(Blind);
        Hypothermia = addon.Bind("Hypothermia", false, new ConfigurableInfo("Hypothermia gradually fades toward that of the least frozen friend in the same room.", tags: ["Hypothermia"]));
        HypothermiaRatio = addon.Bind("HypothermiaRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in hypothermia fades away each frame while a less frozen friend is in the same room. A value of 1 closes the difference at once, and 0 disables the warmth."))
            .Require(Hypothermia);
        Breath = addon.Bind("Breath", false, new ConfigurableInfo("Breath gradually recovers toward that of the friend with the most air in the same room.", tags: ["Breath"]));
        BreathRatio = addon.Bind("BreathRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in breath recovers each frame while a friend with more air is in the same room. A value of 1 closes the difference at once, and 0 disables the recovery."))
            .Require(Breath);
        Forgiveness = addon.Bind("Forgiveness", false, new ConfigurableInfo("Reputation loss between friends is reduced.", tags: ["Forgiveness"]));
        ForgivenessRatio = addon.Bind("ForgivenessRatio", 0.1f, new ConfigurableInfo("Sets how much reputation loss between friends is forgiven. A value of 1 lets the deed pass unnoticed, and 0 leaves the reputation loss untouched."))
            .Require(Forgiveness);
    }
}
