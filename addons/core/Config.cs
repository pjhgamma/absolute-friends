using RippleFriends.Options;

namespace RippleFriends.Core;

public static class Config
{
    public static Configurable<bool> FriendSlugcat = null!;

    public static Configurable<bool> FriendCreature = null!;

    public static Configurable<bool> FriendNeutralCreature = null!;

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
            "Includes players as Ripple Friends, and a creature friendly to one player counts as a Ripple Friend of every player. Ripple Friends never try to attack one another.",
            "Includes players and slugpups as Ripple Friends, and a creature friendly to one player counts as a Ripple Friend of every player. Ripple Friends never try to attack one another."
        ), tags: ["Slugcats"]));
        FriendCreature = addon.Bind("FriendCreature", false, new ConfigurableInfo("Includes friendly creatures, such as lizards tamed by Ripple Friends and friendly scavengers, as Ripple Friends.", tags: ["Friendly Creatures"]));
        FriendNeutralCreature = addon.Bind("FriendNeutralCreature", false, new ConfigurableInfo("Includes neutral creatures, such as neutralized lizards and rain deer, as Ripple Friends alongside friendly ones.", tags: ["Neutral Creatures"]))
            .Require(FriendCreature);

        FriendChaining = addon.Bind("FriendChaining", false, new ConfigurableInfo("Includes the creature Ripple Friends of the same player as one another's Ripple Friends.", tags: ["Friend Chaining"]))
            .Require(FriendSlugcat);
        FriendGrabbed = addon.Bind("FriendGrabbed", false, new ConfigurableInfo("Includes objects grabbed by Ripple Friends as well.", tags: ["Grabbed Objects"]))
            .Require(FriendSlugcat);
        FriendGrabbedForce = addon.Bind("FriendGrabbedForce", true, new ConfigurableInfo("Temporarily excludes a grabbed creature from Ripple Friends while the player Ripple Friend holding it is entering a grab input.", tags: ["Force Grabbing"]))
            .Require(FriendGrabbed);
        FriendArena = addon.Bind("FriendArena", false, new ConfigurableInfo("Activates the Ripple Friends relationship in the Arena.", tags: ["Arena"]));

        FriendLink = addon.Bind("FriendLink", false, new ConfigurableInfo("Draws a line between two things as the mod treats them as Ripple Friends.", tags: ["Friend Link"]));
        FriendName = addon.Bind("FriendName", false, new ConfigurableInfo("Draws the name above every Ripple Friend in view, and keeps a tracked friend's name at the edge of the screen while it is out of view.", tags: ["Friend Name"]));
        FriendIcon = addon.Bind("FriendIcon", false, new ConfigurableInfo("Draws an icon above every Ripple Friend in view, and keeps a tracked friend's icon at the edge of the screen while it is out of view.", tags: ["Friend Icon"]));
        OwnerLink = addon.Bind("OwnerLink", false, new ConfigurableInfo("Draws a line between each object and its owning creature, thickest at the owner's end.", tags: ["Owner Link"]));
        OwnerName = addon.Bind("OwnerName", false, new ConfigurableInfo("Draws the owner's name above everything in view that has an owner.", tags: ["Owner Name"]));
        OwnerIcon = addon.Bind("OwnerIcon", false, new ConfigurableInfo("Draws the owner's icon above everything in view that has an owner.", tags: ["Owner Icon"]));
    }
}
