using AbsoluteFriends.Options;

namespace AbsoluteFriends.Manager;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.SetColumns(2);
        menu.AddKeyBinder(Config.FriendManagerKey, "Friend Manager Shortcut");
        menu.AddCheckBox(Config.TrackNewFriends);
    }
}
