using AbsoluteFriends.Options;

namespace AbsoluteFriends.Core;

public static class Config
{
    internal const string NoConfiguredRule = "__none0";

    public static Configurable<bool> FriendSlugcat = null!;

    public static Configurable<bool> FriendCreature = null!;

    public static Configurable<bool> FriendNeutralCreature = null!;

    internal static Configurable<string> SelectedCreatureType = null!;

    internal static Configurable<string> SelectedCreatureRule = null!;

    internal static Configurable<string> SelectedConfiguredCreatureRules = null!;

    public static Configurable<bool> FriendSharing = null!;

    public static Configurable<bool> FriendChaining = null!;

    public static Configurable<bool> FriendGrabbed = null!;

    public static Configurable<bool> FriendGrabbedForce = null!;

    public static Configurable<bool> FriendStory = null!;

    public static Configurable<bool> FriendExpedition = null!;

    public static Configurable<bool> FriendArena = null!;

    public static Configurable<bool> FriendSafari = null!;

    public static Configurable<bool> FriendLink = null!;

    public static Configurable<bool> FriendName = null!;

    public static Configurable<bool> FriendIcon = null!;

    public static Configurable<bool> OwnerLink = null!;

    public static Configurable<bool> OwnerName = null!;

    public static Configurable<bool> OwnerIcon = null!;

    internal static void Bind(Addon addon)
    {
        FriendSlugcat = addon.Bind("FriendSlugcat", true, new ConfigurableInfo(Options.Config.Downpour(
            "Treats players as friends with one another.",
            "Treats players and slugpups as friends with one another."
        ), tags: ["Slugcats"]));
        FriendCreature = addon.Bind("FriendCreature", false, new ConfigurableInfo("Treats friendly creatures, such as tamed lizards and friendly scavengers, as friends.", tags: ["Friendly Creatures"]));
        FriendNeutralCreature = addon.Bind("FriendNeutralCreature", false, new ConfigurableInfo("Also treats neutral creatures, such as neutralized lizards and rain deer, as friends.", tags: ["Neutral Creatures"]))
            .Require(FriendCreature);

        CreatureRules.Bind(addon);

        SelectedCreatureType = addon.Bind("SelectedCreatureType", CreatureRules.Options.Select(pair => pair.Key).FirstOrDefault() ?? "", new ConfigurableInfo("Choose the creature type to configure.", tags: ["Creature Type"]));
        SelectedCreatureRule = addon.Bind("SelectedCreatureRule", nameof(CreatureRule.Allow), new ConfigurableInfo("Choose when this creature counts as a friend, overriding the global friendship options.", tags: ["Creature Rule"]));
        SelectedConfiguredCreatureRules = addon.Bind("SelectedConfiguredCreatureRules", NoConfiguredRule, new ConfigurableInfo("Select a configured creature rule to edit or remove.", tags: ["Configured Creature Rules"]));

        FriendSharing = addon.Bind("FriendSharing", true, new ConfigurableInfo("Shares a creature's friendship, like, and reputation toward one slugcat with the others. Changes to relationships and reputation remain after disabling this option.", tags: ["Friend Sharing"]));
        FriendChaining = addon.Bind("FriendChaining", false, new ConfigurableInfo("Treats a player's creature friends as friends with one another. Changes to like and knowledge remain after disabling this option.", tags: ["Friend Chaining"]));
        FriendGrabbed = addon.Bind("FriendGrabbed", false, new ConfigurableInfo("Treats objects held by friends as friendly.", tags: ["Grabbed Objects"]));
        FriendGrabbedForce = addon.Bind("FriendGrabbedForce", true, new ConfigurableInfo("Temporarily treats a held creature as not friendly while the player holding it enters a grab input.", tags: ["Force Grabbing"]))
            .Require(FriendGrabbed);
        FriendStory = addon.Bind("FriendStory", true, new ConfigurableInfo("Enables friendship rules in Story mode.", tags: ["Story"]));
        FriendExpedition = addon.Bind("FriendExpedition", true, new ConfigurableInfo("Enables friendship rules in Expedition mode.", tags: ["Expedition"]));
        FriendArena = addon.Bind("FriendArena", false, new ConfigurableInfo("Enables friendship rules in Arena mode.", tags: ["Arena"]));
        FriendSafari = addon.Bind("FriendSafari", false, new ConfigurableInfo("Enables friendship rules in Safari mode.", tags: ["Safari"]));

        FriendLink = addon.Bind("FriendLink", false, new ConfigurableInfo("Draws a line between two things as the mod treats them as friends.", tags: ["Friend Link"]));
        FriendName = addon.Bind("FriendName", false, new ConfigurableInfo("Draws the name above every friend in view, and keeps a tracked friend's name at the edge of the screen while it is out of view.", tags: ["Friend Name"]));
        FriendIcon = addon.Bind("FriendIcon", false, new ConfigurableInfo("Draws an icon above every friend in view, and keeps a tracked friend's icon at the edge of the screen while it is out of view.", tags: ["Friend Icon"]));
        OwnerLink = addon.Bind("OwnerLink", false, new ConfigurableInfo("Draws a line between each object and its owning creature, thickest at the owner's end.", tags: ["Owner Link"]));
        OwnerName = addon.Bind("OwnerName", false, new ConfigurableInfo("Draws the owner's name above everything in view that has an owner.", tags: ["Owner Name"]));
        OwnerIcon = addon.Bind("OwnerIcon", false, new ConfigurableInfo("Draws the owner's icon above everything in view that has an owner.", tags: ["Owner Icon"]));
    }
}
