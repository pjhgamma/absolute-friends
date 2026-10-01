using AbsoluteFriends.Options;

namespace AbsoluteFriends.Players;

internal static class Config
{
    public static Configurable<bool> GrabbingPlayer = null!;

    public static Configurable<float> GrabbingPlayerTime = null!;

    public static Configurable<bool> Wiggle = null!;

    public static Configurable<bool> Carry = null!;

    public static Configurable<bool> CarryStun = null!;

    public static Configurable<bool> Mauling = null!;

    public static Configurable<bool> GourmandSlam = null!;

    public static Configurable<bool> ArtificerParry = null!;

    public static Configurable<bool> SaintTongue = null!;

    public static Configurable<bool> SaintAttunement = null!;

    public static Configurable<bool> WatcherCamouflage = null!;

    public static Configurable<bool> RippleSpace = null!;

    public static void Bind(Addon addon)
    {
        GrabbingPlayer = addon.Bind("GrabbingPlayer", true, new ConfigurableInfo("Will not be grabbed by player friends while entering control inputs.", tags: ["Grab Player"]))
            .Require(Core.Config.FriendSlugcat);
        GrabbingPlayerTime = addon.Bind("GrabbingPlayerTime", 1f, new ConfigurableInfo("Sets the maximum control input time (in seconds) during which a player friend cannot be grabbed."))
            .Require(GrabbingPlayer);
        Wiggle = addon.Bind("Wiggle", true, new ConfigurableInfo("Can wiggle free from the grasp of player friends.", tags: ["Wiggle"]))
            .Require(Core.Config.FriendSlugcat);
        Carry = addon.Bind("Carry", true, new ConfigurableInfo("Can pick up and carry large tracked friends with both hands.", tags: ["Carry"]));
        CarryStun = addon.Bind("CarryStun", true, new ConfigurableInfo("Keeps a carried tracked friend stunned until released.", tags: ["Carry Stun"]))
            .Require(Carry);
        Mauling = addon.Bind("Mauling", true, new ConfigurableInfo(Options.Config.Downpour(
            "Will not eat friends.",
            "Will not maul or eat friends."
        ), tags: ["Mauling"]));
        GourmandSlam = addon.Bind("GourmandSlam", true, new ConfigurableInfo(Options.Config.JollyCoop(
            "Will not take damage from the roll, slide, or slam of a Gourmand friend.",
            "Will not take damage from the roll, slide, or slam of a Gourmand friend. If the Spears Miss option is enabled in Jolly Co-op, slugcats will never damage each other."
        ), tags: ["Gourmand Slam"]));
        ArtificerParry = addon.Bind("ArtificerParry", true, new ConfigurableInfo(Options.Config.JollyCoop(
            "Will not be stunned by the parry of an Artificer friend.",
            "Will not be stunned by the parry of an Artificer friend. If the Spears Miss option is enabled in Jolly Co-op, slugcats will never stun each other."
        ), tags: ["Artificer Parry"]));
        SaintTongue = addon.Bind("SaintTongue", true, new ConfigurableInfo("Will not be caught by the tongue of a Saint friend.", tags: ["Saint Tongue"]));
        SaintAttunement = addon.Bind("SaintAttunement", true, new ConfigurableInfo("Will not be instantly killed by the attunement of a Saint friend.", tags: ["Saint Attunement"]));
        WatcherCamouflage = addon.Bind("WatcherCamouflage", true, new ConfigurableInfo("Shares camouflage and its gauge with Watcher friends. Forced camouflage changes remain personal until synchronization resumes.", tags: ["Watcher Camouflage"]));
        RippleSpace = addon.Bind("RippleSpace", true, new ConfigurableInfo("Tracked friends follow their player into ripple space and stay in view.", tags: ["Ripple Space"]));
    }
}
