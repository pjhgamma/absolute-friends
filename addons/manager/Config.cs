using UnityEngine;

namespace AbsoluteFriends.Manager;

internal static class Config
{
    public static Configurable<KeyCode> FriendManagerKey = null!;

    public static Configurable<bool> TrackNewFriends = null!;

    public static void Bind(Addon addon)
    {
        FriendManagerKey = addon.Bind("FriendManagerKey", KeyCode.F7, new ConfigurableInfo("Opens the friend manager during gameplay or while paused.", tags: ["Friend Manager Shortcut"]));
        TrackNewFriends = addon.Bind("TrackNewFriends", true, new ConfigurableInfo("Tracks newly tamed friends. When off, new friends start untracked.", tags: ["Track New Friends"]));

        addon.Group("Track New Friends", "Tracks newly tamed friends. When off, new friends start untracked.", TrackNewFriends);
    }
}
