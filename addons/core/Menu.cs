using RippleFriends.Options;

namespace RippleFriends.Core;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddLabel("Ripple Friends do not interfere with each other's trajectories.");
        menu.AddLabel("Ripples flow bidirectionally to one another, excluding oneself.");

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
