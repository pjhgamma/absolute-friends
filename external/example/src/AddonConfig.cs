using AbsoluteFriends.Addons;
using AbsoluteFriends.Options;

namespace AbsoluteFriendsExample;

// A separate config class is only an organizational choice; addon options may be bound elsewhere.
internal static class AddonConfig
{
    internal static Configurable<bool> Friendship = null!;

    internal static Configurable<bool> AggressiveHunter = null!;

    internal static Configurable<bool> HunterDaddyOwnership = null!;

    internal static Configurable<bool> UntrackedHunter = null!;

    internal static Configurable<bool> HunterName = null!;

    internal static Configurable<bool> GreenNeuronOwnership = null!;

    internal static void Bind(Addon addon)
    {
        // Addon.Bind makes settings profile-aware. Require affects both the UI and runtime IsActive checks.
        Friendship = addon.Bind("Friendship", true, new ConfigurableInfo("Treats Hunter Long Legs as a friend of players.", tags: ["Hunter Long Legs"]))
            .Require(AbsoluteFriends.Core.Config.FriendCreature);
        AggressiveHunter = addon.Bind("AggressiveHunter", true, new ConfigurableInfo("Preserves Hunter's hostile relationship with Hunter Long Legs.", tags: ["Aggressive Hunter"]))
            .Require(Friendship);
        HunterDaddyOwnership = addon.Bind("HunterDaddyOwnership", true, new ConfigurableInfo("Makes Hunter Long Legs own anything held by its tentacles.", tags: ["Tentacle Ownership"]));
        UntrackedHunter = addon.Bind("UntrackedHunter", true, new ConfigurableInfo("Keeps Hunter Long Legs untracked, so other addons do not treat it as a companion.", tags: ["Untracked Long Legs"]))
            .Require(Friendship);
        HunterName = addon.Bind("HunterName", true, new ConfigurableInfo("Gives each Hunter Long Legs a name.", tags: ["Long Legs Name"]));
        GreenNeuronOwnership = addon.Bind("GreenNeuronOwnership", true, new ConfigurableInfo("Makes every green Neuron self-owned.", tags: ["Green Neuron Ownership"]));

        // Group puts one switch in the Simple view of the framework's Addons tab that turns all of its options on or off together.
        // An addon may group only its own Boolean options, and each option may belong to only one group.
        // Options left out of every group appear only in the Detailed view.
        addon.Group("Hunter Long Legs", "Treats Hunter Long Legs as a friend and lets it own what it holds.", Friendship, AggressiveHunter, HunterDaddyOwnership);
        // Keep an option that turns a feature off, such as UntrackedHunter, out of feature groups, because a group switch turns all of its options on together.
        addon.Group("Untracked Long Legs", "Keeps Hunter Long Legs untracked, so other addons do not treat it as a companion.", UntrackedHunter);
        addon.Group("Long Legs Name", "Gives each Hunter Long Legs a name.", HunterName);
        addon.Group("Green Neuron Ownership", "Makes every green Neuron self-owned.", GreenNeuronOwnership);
    }
}
