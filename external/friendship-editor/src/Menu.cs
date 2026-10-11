using AbsoluteFriends.Options;

namespace AbsoluteFriends.FriendshipEditor;

internal static class Menu
{
    internal static void Build(MenuBuilder menu)
    {
        menu.SetColumns(3);
        menu.AddCheckBox(Config.ReputationEditor);
        menu.AddCheckBox(Config.LikeEditor);
        menu.AddCheckBox(Config.RelationshipEditor);
    }
}
