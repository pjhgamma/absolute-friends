using AbsoluteFriends.Addons;
using AbsoluteFriends.Options;

namespace AbsoluteFriendsExample;

// A separate config class is only an organizational choice; addon options may be bound elsewhere.
internal static class AddonConfig
{
    internal static Configurable<bool> Friendship = null!;

    internal static Configurable<bool> AggressiveHunter = null!;

    internal static Configurable<bool> HunterDaddyOwnership = null!;

    internal static Configurable<bool> GreenNeuronOwnership = null!;

    internal static void Bind(Addon addon)
    {
        // Addon.Bind makes settings profile-aware. Require affects both the UI and runtime IsActive checks.
        Friendship = addon.Bind("Friendship", true, new ConfigurableInfo("Treats Hunter Long Legs as a friend of players.", tags: ["Hunter Long Legs"]))
            .Require(AbsoluteFriends.Core.Config.FriendCreature);
        AggressiveHunter = addon.Bind("AggressiveHunter", true, new ConfigurableInfo("Preserves Hunter's hostile relationship with Hunter Long Legs.", tags: ["Aggressive Hunter"]))
            .Require(Friendship);
        HunterDaddyOwnership = addon.Bind("HunterDaddyOwnership", true, new ConfigurableInfo("Makes Hunter Long Legs own anything held by its tentacles.", tags: ["Tentacle Ownership"]));
        GreenNeuronOwnership = addon.Bind("GreenNeuronOwnership", true, new ConfigurableInfo("Makes every green Neuron self-owned.", tags: ["Green Neuron Ownership"]));

        // Group puts one switch in the Simple view of the framework's Addons tab that turns all of its options on or off together.
        // An addon may group only its own Boolean options, and each option may belong to only one group.
        // Options left out of every group appear only in the Detailed view.
        addon.Group("Hunter Long Legs", "Treats Hunter Long Legs as a friend and lets it own what it holds.", Friendship, AggressiveHunter, HunterDaddyOwnership);
        addon.Group("Green Neuron Ownership", "Makes every green Neuron self-owned.", GreenNeuronOwnership);
    }
}
