using AbsoluteFriends.Addons;

namespace AbsoluteFriends.FriendshipEditor;

internal static class Config
{
    internal static Configurable<bool> ReputationEditor = null!;

    internal static Configurable<bool> LikeEditor = null!;

    internal static void Bind(Addon addon)
    {
        ReputationEditor = addon.Bind("ReputationEditor", true, new ConfigurableInfo("Adds the Reputation tab to players.", tags: ["Reputation"]));
        LikeEditor = addon.Bind("LikeEditor", true, new ConfigurableInfo("Adds the Like tab to tracked friends.", tags: ["Like"]));

        addon.Group("Friendship Editor", "Adds the Reputation and Like tabs to the friend manager.", ReputationEditor, LikeEditor);
    }
}
