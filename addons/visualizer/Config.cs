using AbsoluteFriends.Options;

namespace AbsoluteFriends.Visualizer;

internal static class Config
{
    public static Configurable<bool> FriendLink = null!;

    public static Configurable<bool> FriendName = null!;

    public static Configurable<bool> FriendIcon = null!;

    public static Configurable<bool> OwnerLink = null!;

    public static Configurable<bool> OwnerName = null!;

    public static Configurable<bool> OwnerIcon = null!;

    public static Configurable<int> UpdateInterval = null!;

    public static void Bind(Addon addon)
    {
        FriendLink = addon.Bind("FriendLink", false, new ConfigurableInfo("Draws a line between two things as the mod treats them as friends.", tags: ["Friend Link"]));
        FriendName = addon.Bind("FriendName", false, new ConfigurableInfo("Draws the name above every friend in view, and keeps a tracked friend's name at the edge of the screen while it is out of view.", tags: ["Friend Name"]));
        FriendIcon = addon.Bind("FriendIcon", false, new ConfigurableInfo("Draws an icon above every friend in view, and keeps a tracked friend's icon at the edge of the screen while it is out of view.", tags: ["Friend Icon"]));
        OwnerLink = addon.Bind("OwnerLink", false, new ConfigurableInfo("Draws a line between each object and its owning creature, thickest at the owner's end.", tags: ["Owner Link"]));
        OwnerName = addon.Bind("OwnerName", false, new ConfigurableInfo("Draws the owner's name above everything in view that has an owner.", tags: ["Owner Name"]));
        OwnerIcon = addon.Bind("OwnerIcon", false, new ConfigurableInfo("Draws the owner's icon above everything in view that has an owner.", tags: ["Owner Icon"]));
        UpdateInterval = addon.Bind("UpdateInterval", 10, new ConfigurableInfo("Sets how many frames pass between friend and owner checks.", new ConfigAcceptableRange<int>(1, 40), tags: ["Update Interval"]))
            .RequireAny(FriendLink, FriendName, FriendIcon, OwnerLink, OwnerName, OwnerIcon);
    }
}
