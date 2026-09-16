using RippleFriends.Options;

namespace RippleFriends.Players;

internal static class Config
{
    public static Configurable<bool> GrabbingPlayer = null!;

    public static Configurable<float> GrabbingPlayerTime = null!;

    public static Configurable<bool> Wiggle = null!;

    public static Configurable<bool> Carry = null!;

    public static Configurable<bool> Mauling = null!;

    public static Configurable<bool> GourmandSlam = null!;

    public static Configurable<bool> ArtificerParry = null!;

    public static Configurable<bool> SaintTongue = null!;

    public static Configurable<bool> SaintAttunement = null!;

    public static Configurable<bool> WatcherRipple = null!;

    public static void Bind(Addon addon)
    {
        GrabbingPlayer = addon.Bind("GrabbingPlayer", true, new ConfigurableInfo("Will not be grabbed by player Ripple Friends while entering control inputs.", tags: ["Grab Player"]))
            .Require(Core.Config.FriendSlugcat);
        GrabbingPlayerTime = addon.Bind("GrabbingPlayerTime", 1f, new ConfigurableInfo("Sets the maximum control input time (in seconds) during which a player Ripple Friend cannot be grabbed."))
            .Require(GrabbingPlayer);
        Wiggle = addon.Bind("Wiggle", true, new ConfigurableInfo("Can wiggle free from the grasp of player Ripple Friends.", tags: ["Wiggle"]))
            .Require(Core.Config.FriendSlugcat);
        Carry = addon.Bind("Carry", false, new ConfigurableInfo("Can pick up and carry tracked Ripple Friends that are otherwise too large to be held. A carried Ripple Friend rests still, and stirs again once it is let go.", tags: ["Carry"]))
            .Require(Core.Config.FriendSlugcat);
        Mauling = addon.Bind("Mauling", false, new ConfigurableInfo(Options.Config.Downpour(
            "Will not eat Ripple Friends.",
            "Will not maul or eat Ripple Friends."
        ), tags: ["Mauling"]))
            .Require(Core.Config.FriendSlugcat);
        GourmandSlam = addon.Bind("GourmandSlam", false, new ConfigurableInfo(Options.Config.JollyCoop(
            "Will not take damage from the roll, slide, or slam of a Gourmand Ripple Friend.",
            "Will not take damage from the roll, slide, or slam of a Gourmand Ripple Friend. If the Spears Miss option is enabled in Jolly Co-op, slugcats will never damage each other."
        ), tags: ["Gourmand Slam"]))
            .Require(Core.Config.FriendSlugcat);
        ArtificerParry = addon.Bind("ArtificerParry", false, new ConfigurableInfo(Options.Config.JollyCoop(
            "Will not be stunned by the parry of an Artificer Ripple Friend.",
            "Will not be stunned by the parry of an Artificer Ripple Friend. If the Spears Miss option is enabled in Jolly Co-op, slugcats will never stun each other."
        ), tags: ["Artificer Parry"]))
            .Require(Core.Config.FriendSlugcat);
        SaintTongue = addon.Bind("SaintTongue", false, new ConfigurableInfo("Will not be caught by the tongue of a Saint Ripple Friend.", tags: ["Saint Tongue"]))
            .Require(Core.Config.FriendSlugcat);
        SaintAttunement = addon.Bind("SaintAttunement", false, new ConfigurableInfo("Will not be instantly killed by the attunement of a Saint Ripple Friend.", tags: ["Saint Attunement"]))
            .Require(Core.Config.FriendSlugcat);
        WatcherRipple = addon.Bind("WatcherRipple", false, new ConfigurableInfo("Will share camouflage and its gauge with Watcher Ripple Friends, yet what the game forces off is not shared, and synchronized later. Tracked Ripple Friends also stay in their player's ripple space and in sight.", tags: ["Watcher Ripple"]))
            .Require(Core.Config.FriendSlugcat);
    }
}
