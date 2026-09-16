using RippleFriends.Options;

namespace RippleFriends.Creatures;

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
        LizardBite = addon.Bind("LizardBite", false, new ConfigurableInfo("Will not be bitten by lizard Ripple Friends.", tags: ["Lizard Bite"]))
            .Require(Core.Config.FriendCreature);
        LizardTongue = addon.Bind("LizardTongue", false, new ConfigurableInfo("Will not be caught by the tongue of a lizard Ripple Friend. Instead, an indigo lizard Ripple Friend licks any Ripple Friend caught in a locust swarm, not only the one it follows.", tags: ["Lizard Tongue"]))
            .Require(Core.Config.FriendCreature);
        LizardSpit = addon.Bind("LizardSpit", false, new ConfigurableInfo("Will not be stuck by the spit of a lizard Ripple Friend.", tags: ["Lizard Spit"]))
            .Require(Core.Config.FriendCreature);
        LizardBeam = addon.Bind("LizardBeam", false, new ConfigurableInfo("Will not be struck by the laser of a blizzard lizard Ripple Friend.", tags: ["Lizard Laser"]))
            .Require(Core.Config.FriendCreature);
        LizardBlizzard = addon.Bind("LizardBlizzard", false, new ConfigurableInfo("Will not be swept up by the shield around a blizzard lizard Ripple Friend.", tags: ["Lizard Shield"]))
            .Require(Core.Config.FriendCreature);
        LizardPoison = addon.Bind("LizardPoison", false, new ConfigurableInfo("Will not be poisoned by the touch of a basilisk lizard Ripple Friend, nor dazed by the mushroom haze around it.", tags: ["Lizard Poison"]))
            .Require(Core.Config.FriendCreature);

        ScavengerShelter = addon.Bind("ScavengerShelter", false, new ConfigurableInfo("Will not have objects inside a shelter taken by scavenger Ripple Friends.", tags: ["Scavenger Shelter"]))
            .Require(Core.Config.FriendCreature);
        ScavengerTemplar = addon.Bind("ScavengerTemplar", false, new ConfigurableInfo("Will not be struck by the shockwave from the karma shield of a templar Ripple Friend.", tags: ["Scavenger Shield"]))
            .Require(Core.Config.FriendCreature);
    }
}
