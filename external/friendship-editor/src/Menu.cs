using AbsoluteFriends.Options;

namespace AbsoluteFriends.FriendshipEditor;

internal static class Menu
{
    internal static void Build(MenuBuilder menu)
    {
        menu.SetColumns(2);
        menu.AddCheckBox(Config.ReputationEditor);
        menu.AddCheckBox(Config.LikeEditor);
    }
}
