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
        menu.AddCheckBox(Config.FriendSharing);
        menu.AddCheckBox(Config.FriendChaining);
        menu.AddCheckBox(Config.FriendGrabbed);
        menu.AddCheckBox(Config.FriendGrabbedForce);

        menu.AddTitle("Sessions");
        menu.SetColumns(4);
        menu.AddCheckBox(Config.FriendStory);
        menu.AddCheckBox(Config.FriendExpedition, enabled: ModManager.Expedition);
        menu.AddCheckBox(Config.FriendArena);
        menu.AddCheckBox(Config.FriendSafari, enabled: ModManager.MSC);

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
