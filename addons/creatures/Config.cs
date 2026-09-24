using AbsoluteFriends.Options;

namespace AbsoluteFriends.Creatures;

internal static class Config
{
    public static Configurable<bool> LizardBite = null!;

    public static Configurable<bool> LizardTongue = null!;

    public static Configurable<bool> LizardSpit = null!;

    public static Configurable<bool> LizardBeam = null!;

    public static Configurable<bool> LizardBlizzard = null!;

    public static Configurable<bool> LizardPoison = null!;

    public static Configurable<bool> ScavengerShelter = null!;

    public static Configurable<bool> ScavengerTemplar = null!;

    internal static void Bind(Addon addon)
    {
        LizardBite = addon.Bind("LizardBite", false, new ConfigurableInfo("Will not be bitten by friendly lizards.", tags: ["Lizard Bite"]))
            .Require(Core.Config.FriendCreature);
        LizardTongue = addon.Bind("LizardTongue", false, new ConfigurableInfo("Will not be caught by a friendly lizard's tongue. Instead, a friendly indigo lizard licks any friend caught in a locust swarm, not only the one it follows.", tags: ["Lizard Tongue"]))
            .Require(Core.Config.FriendCreature);
        LizardSpit = addon.Bind("LizardSpit", false, new ConfigurableInfo("Will not be stuck by a friendly lizard's spit.", tags: ["Lizard Spit"]))
            .Require(Core.Config.FriendCreature);
        LizardBeam = addon.Bind("LizardBeam", false, new ConfigurableInfo("Will not be struck by a friendly blizzard lizard's laser.", tags: ["Lizard Laser"]))
            .Require(Core.Config.FriendCreature);
        LizardBlizzard = addon.Bind("LizardBlizzard", false, new ConfigurableInfo("Will not be swept up by a friendly blizzard lizard's shield.", tags: ["Lizard Shield"]))
            .Require(Core.Config.FriendCreature);
        LizardPoison = addon.Bind("LizardPoison", false, new ConfigurableInfo("Will not be poisoned by a friendly basilisk lizard's touch, nor dazed by its mushroom effect.", tags: ["Lizard Poison"]))
            .Require(Core.Config.FriendCreature);

        ScavengerShelter = addon.Bind("ScavengerShelter", false, new ConfigurableInfo("Will not have shelter objects taken by friendly scavengers.", tags: ["Scavenger Shelter"]))
            .Require(Core.Config.FriendCreature);
        ScavengerTemplar = addon.Bind("ScavengerTemplar", false, new ConfigurableInfo("Will not be struck by a friendly scavenger's karma shield shockwave.", tags: ["Scavenger Shield"]))
            .Require(Core.Config.FriendCreature);
    }
}
