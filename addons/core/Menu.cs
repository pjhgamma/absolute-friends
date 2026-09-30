using AbsoluteFriends.Options;

namespace AbsoluteFriends.Core;

internal static partial class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Friendship");
        menu.AddLabel("Friendship works both ways, but nothing is its own friend.");
        menu.AddLabel("Creature rules override the Friendly Creatures and Neutral Creatures options.");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.FriendSlugcat);
        menu.AddCheckBox(Config.FriendCreature);
        menu.AddCheckBox(Config.FriendNeutralCreature);

        BuildCreatureTypes(menu);

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

        menu.AddTitle("Friend Manager");
        menu.AddRow(0.5f);
        menu.SetColumns(2);
        menu.AddKeyBinder(Config.FriendManagerKey, "Friend Manager Shortcut");
    }
}
