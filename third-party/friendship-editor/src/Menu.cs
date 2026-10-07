using AbsoluteFriends.Options;

namespace AbsoluteFriends.FriendshipEditor;

internal static class Menu
{
    internal static void Build(MenuBuilder menu)
    {
        menu.AddLabel("Open the friend manager and select a player or a tracked friend to edit friendship.");
        menu.SetColumns(2);
        menu.AddCheckBox(Config.ReputationEditor);
        menu.AddCheckBox(Config.LikeEditor);
    }
}
