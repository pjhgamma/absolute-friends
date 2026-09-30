using AbsoluteFriends.Options;

namespace AbsoluteFriends.Visualizer;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.SetColumns(3);
        menu.AddCheckBox(Config.FriendLink);
        menu.AddCheckBox(Config.FriendName);
        menu.AddCheckBox(Config.FriendIcon);
        menu.AddCheckBox(Config.OwnerLink);
        menu.AddCheckBox(Config.OwnerName);
        menu.AddCheckBox(Config.OwnerIcon);
        menu.SetColumns(4);
        menu.AddIntSlider(Config.UpdateInterval, "Update Interval", span: 4f, min: 1, max: 40);
    }
}
