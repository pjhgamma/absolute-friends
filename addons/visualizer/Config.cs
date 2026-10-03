using AbsoluteFriends.Options;

namespace AbsoluteFriends.Visualizer;

internal static class Config
{
    public static Configurable<bool> Icons = null!;

    public static Configurable<int> IconStyle = null!;

    public static Configurable<bool> Names = null!;

    public static Configurable<bool> Links = null!;

    public static Configurable<int> ShownFriendship = null!;

    public static Configurable<bool> Owners = null!;

    public static Configurable<int> UpdateInterval = null!;

    public static void Bind(Addon addon)
    {
        Icons = addon.Bind("Icons", false, new ConfigurableInfo("Draws icons above shown friends, and owners' icons above owned things. Dead creatures' icons are dimmed.", tags: ["Relationship Icons"]));
        IconStyle = addon.Bind("IconStyle", (int)RelationshipIconStyle.Creature, new ConfigurableInfo("Chooses creature artwork or friendship symbols for relationship icons.", new ConfigAcceptableRange<int>(0, 1), tags: ["Icon Style"]))
            .Require(Icons);
        Names = addon.Bind("Names", false, new ConfigurableInfo("Draws names above shown friends, and owners' names above owned things. Dead creatures' names are dimmed.", tags: ["Relationship Names"]));
        Links = addon.Bind("Links", false, new ConfigurableInfo("Draws lines between shown friends and between owners and owned things. Ownership lines are thickest at the owner's end.", tags: ["Relationship Links"]));

        ShownFriendship = addon.Bind("ShownFriendship", (int)ShownFriendshipChoice.Indirect, new ConfigurableInfo("Sets the weakest friendship that relationship overlays show. Moving right shows more friends.", new ConfigAcceptableRange<int>(0, 3), tags: ["Shown Friendship"]))
            .RequireAny(Icons, Names, Links);
        Owners = addon.Bind("Owners", true, new ConfigurableInfo("Includes ownership in relationship overlays.", tags: ["Owner Relationships"]))
            .RequireAny(Icons, Names, Links);

        UpdateInterval = addon.Bind("UpdateInterval", 10, new ConfigurableInfo("Sets how many frames pass between friend and owner checks.", new ConfigAcceptableRange<int>(1, 40), tags: ["Update Interval"]))
            .RequireAny(Icons, Names, Links);

        addon.Group("Relationship Overlay", "Visualizes relationships with icons, names, and links.", Icons, Names, Links);
    }
}
