using AbsoluteFriends.Addons;

namespace AbsoluteFriends.FriendshipEditor;

internal static class Config
{
    internal static Configurable<bool> ReputationEditor = null!;

    internal static Configurable<bool> LikeEditor = null!;

    internal static Configurable<bool> RelationshipEditor = null!;

    internal static void Bind(Addon addon)
    {
        ReputationEditor = addon.Bind("ReputationEditor", true, new ConfigurableInfo("Sets reputation rules for players.", tags: ["Reputation"]));
        LikeEditor = addon.Bind("LikeEditor", true, new ConfigurableInfo("Sets like rules toward tracked friends.", tags: ["Like"]));
        RelationshipEditor = addon.Bind("RelationshipEditor", true, new ConfigurableInfo("Sets relationship rules toward tracked friends.", tags: ["Relationship"]));

        addon.Group("Friendship Editor", "Adds the Friendship tab to the friend manager.", ReputationEditor, LikeEditor, RelationshipEditor);
    }
}
