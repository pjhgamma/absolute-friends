using AbsoluteFriends.Options;

namespace AbsoluteFriends.Core;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddLabel("Friendship applies in both directions, but nothing counts as its own friend.");

        menu.AddTitle("Base");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.FriendSlugcat);
        menu.AddCheckBox(Config.FriendCreature);
        menu.AddCheckBox(Config.FriendNeutralCreature);

        menu.AddTitle("Extended");
        menu.SetColumns(4);
        menu.AddCheckBox(Config.FriendChaining);
        menu.AddCheckBox(Config.FriendGrabbed);
        menu.AddCheckBox(Config.FriendGrabbedForce);
        menu.AddCheckBox(Config.FriendArena);

        menu.AddTitle("Visualizer");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.FriendLink);
        menu.AddCheckBox(Config.FriendName);
        menu.AddCheckBox(Config.FriendIcon);
        menu.AddCheckBox(Config.OwnerLink);
        menu.AddCheckBox(Config.OwnerName);
        menu.AddCheckBox(Config.OwnerIcon);
    }
}
