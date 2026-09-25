using AbsoluteFriends.Options;

namespace AbsoluteFriends.Core;

public static class Config
{
    public static Configurable<bool> FriendSlugcat = null!;

    public static Configurable<bool> FriendCreature = null!;

    public static Configurable<bool> FriendNeutralCreature = null!;

    public static Configurable<bool> FriendSharing = null!;

    public static Configurable<bool> FriendChaining = null!;

    public static Configurable<bool> FriendGrabbed = null!;

    public static Configurable<bool> FriendGrabbedForce = null!;

    public static Configurable<bool> FriendArena = null!;

    public static Configurable<bool> FriendLink = null!;

    public static Configurable<bool> FriendName = null!;

    public static Configurable<bool> FriendIcon = null!;

    public static Configurable<bool> OwnerLink = null!;

    public static Configurable<bool> OwnerName = null!;

    public static Configurable<bool> OwnerIcon = null!;

    internal static void Bind(Addon addon)
    {
        FriendSlugcat = addon.Bind("FriendSlugcat", true, new ConfigurableInfo(Options.Config.Downpour(
            "Treats players as friends and keeps friends from targeting one another.",
            "Treats players and slugpups as friends and keeps friends from targeting one another."
        ), tags: ["Slugcats"]));
        FriendCreature = addon.Bind("FriendCreature", false, new ConfigurableInfo("Treats friendly creatures, such as tamed lizards and friendly scavengers, as friends.", tags: ["Friendly Creatures"]));
        FriendNeutralCreature = addon.Bind("FriendNeutralCreature", false, new ConfigurableInfo("Treats neutral creatures, such as neutralized lizards and rain deer, as friends.", tags: ["Neutral Creatures"]))
            .Require(FriendCreature);

        FriendSharing = addon.Bind("FriendSharing", true, new ConfigurableInfo("Shares a creature's friendship, like, and reputation toward one slugcat with the others.", tags: ["Friend Sharing"]))
            .Require(FriendCreature);
        FriendChaining = addon.Bind("FriendChaining", false, new ConfigurableInfo("Treats a player's creature friends as friends with one another.", tags: ["Friend Chaining"]));
        FriendGrabbed = addon.Bind("FriendGrabbed", false, new ConfigurableInfo("Treats objects held by friends as friendly.", tags: ["Grabbed Objects"]));
        FriendGrabbedForce = addon.Bind("FriendGrabbedForce", true, new ConfigurableInfo("Temporarily treats a held creature as not friendly while the player holding it enters a grab input.", tags: ["Force Grabbing"]))
            .Require(FriendGrabbed);
        FriendArena = addon.Bind("FriendArena", false, new ConfigurableInfo("Enables friendship rules in the Arena.", tags: ["Arena"]));

        FriendLink = addon.Bind("FriendLink", false, new ConfigurableInfo("Draws a line between two things as the mod treats them as friends.", tags: ["Friend Link"]));
        FriendName = addon.Bind("FriendName", false, new ConfigurableInfo("Draws the name above every friend in view, and keeps a tracked friend's name at the edge of the screen while it is out of view.", tags: ["Friend Name"]));
        FriendIcon = addon.Bind("FriendIcon", false, new ConfigurableInfo("Draws an icon above every friend in view, and keeps a tracked friend's icon at the edge of the screen while it is out of view.", tags: ["Friend Icon"]));
        OwnerLink = addon.Bind("OwnerLink", false, new ConfigurableInfo("Draws a line between each object and its owning creature, thickest at the owner's end.", tags: ["Owner Link"]));
        OwnerName = addon.Bind("OwnerName", false, new ConfigurableInfo("Draws the owner's name above everything in view that has an owner.", tags: ["Owner Name"]));
        OwnerIcon = addon.Bind("OwnerIcon", false, new ConfigurableInfo("Draws the owner's icon above everything in view that has an owner.", tags: ["Owner Icon"]));
    }
}
