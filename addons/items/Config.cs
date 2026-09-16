using RippleFriends.Options;

namespace RippleFriends.Items;

internal static class Config
{
    public static Configurable<bool> Rock = null!;

    public static Configurable<bool> Spear = null!;

    public static Configurable<bool> ExplosiveSpear = null!;

    public static Configurable<bool> ElectricSpear = null!;

    public static Configurable<bool> HellSpear = null!;

    public static Configurable<bool> PoisonSpear = null!;

    public static Configurable<bool> LilyPuck = null!;

    public static Configurable<bool> ScavengerBomb = null!;

    public static Configurable<bool> SingularityBomb = null!;

    public static Configurable<bool> FireEgg = null!;

    public static Configurable<bool> SporePlant = null!;

    public static Configurable<bool> Boomerang = null!;

    public static Configurable<bool> Mushroom = null!;

    public static Configurable<bool> FlareBomb = null!;

    public static Configurable<bool> PuffBall = null!;

    public static Configurable<bool> WaterNut = null!;

    public static Configurable<bool> FirecrackerPlant = null!;

    public static Configurable<bool> GraffitiBomb = null!;

    public static Configurable<bool> DangleFruit = null!;

    public static Configurable<bool> JellyFish = null!;

    public static Configurable<bool> Pomegranate = null!;

    public static Configurable<bool> Snail = null!;

    public static Configurable<bool> TubeWorm = null!;

    public static Configurable<bool> Frog = null!;

    public static void Bind(Addon addon)
    {
        Rock = addon.Bind("Rock", true, new ConfigurableInfo(Options.Config.Downpour(
            "Will not be hit by a rock thrown by Ripple Friends.",
            "Will not be hit by a rock thrown by Ripple Friends. This also applies to joke rifle bullets."
        ), tags: ["Rock"]));
        Spear = addon.Bind("Spear", true, new ConfigurableInfo("Will not be hit by a spear thrown by Ripple Friends.", tags: ["Spear"]));
        ExplosiveSpear = addon.Bind("ExplosiveSpear", true, new ConfigurableInfo("Will not be hit by an explosive spear thrown by Ripple Friends. However, this does not apply to the explosion.", tags: ["Explosive Spear"]));
        ElectricSpear = addon.Bind("ElectricSpear", true, new ConfigurableInfo("Will not be hit by an electric spear thrown by Ripple Friends.", tags: ["Electric Spear"]));
        HellSpear = addon.Bind("HellSpear", true, new ConfigurableInfo("Will not be hit by a fire spear thrown by Ripple Friends.", tags: ["Fire Spear"]));
        PoisonSpear = addon.Bind("PoisonSpear", true, new ConfigurableInfo("Will not be hit by a poison spear thrown by Ripple Friends.", tags: ["Poison Spear"]));
        LilyPuck = addon.Bind("LilyPuck", true, new ConfigurableInfo("Will not be hit by a lilypuck thrown by Ripple Friends.", tags: ["Lilypuck"]));
        ScavengerBomb = addon.Bind("ScavengerBomb", false, new ConfigurableInfo(Options.Config.Downpour(
            "Will not be hit by a grenade thrown by Ripple Friends. However, this does not apply to the explosion.",
            "Will not be hit by a grenade thrown by Ripple Friends. However, this does not apply to the explosion. This also applies to joke rifle bullets."
        ), tags: ["Grenade"]));
        SingularityBomb = addon.Bind("SingularityBomb", false, new ConfigurableInfo("Will not be sucked in or instantly killed by a singularity bomb thrown by Ripple Friends. However, this does not apply to the explosion. This also applies to joke rifle bullets.", tags: ["Singularity Bomb"]));
        FireEgg = addon.Bind("FireEgg", false, new ConfigurableInfo("Will not be attached by a fire egg thrown by Ripple Friends. However, this does not apply to the explosion. This also applies to joke rifle bullets.", tags: ["Fire Egg"]));
        SporePlant = addon.Bind("SporePlant", false, new ConfigurableInfo(Options.Config.Downpour(
            "Will not be caught by bees triggered by Ripple Friends. However, this does not apply to those spawned by approaching a beehive.",
            "Will not be caught by bees triggered by Ripple Friends. However, this does not apply to those spawned by approaching a beehive. This also applies to joke rifle bullets."
        ), tags: ["Beehive"]));
        Boomerang = addon.Bind("Boomerang", false, new ConfigurableInfo("Will not be hit by a boomerang thrown by Ripple Friends.", tags: ["Boomerang"]));
        Mushroom = addon.Bind("Mushroom", false, new ConfigurableInfo("Will share the effects of mushrooms with slugcat Ripple Friends.", tags: ["Mushroom"]))
            .Require(Core.Config.FriendSlugcat);
        FlareBomb = addon.Bind("FlareBomb", false, new ConfigurableInfo(Options.Config.Downpour(
            "Will not be hit by a flash bang thrown by Ripple Friends.",
            "Will not be hit by a flash bang thrown by Ripple Friends. This also applies to joke rifle bullets."
        ), tags: ["Flash Bang"]));
        PuffBall = addon.Bind("PuffBall", false, new ConfigurableInfo(Options.Config.Downpour(
            "Will not be hit by a spore puff thrown by Ripple Friends.",
            "Will not be hit by a spore puff thrown by Ripple Friends. This also applies to joke rifle bullets."
        ), tags: ["Spore Puff"]));
        WaterNut = addon.Bind("WaterNut", false, new ConfigurableInfo("Will not be hit by an unswollen bubble fruit thrown by Ripple Friends.", tags: ["Bubble Fruit"]));
        FirecrackerPlant = addon.Bind("FirecrackerPlant", false, new ConfigurableInfo(Options.Config.Downpour(
            "Will not be stunned by a cherrybomb thrown by Ripple Friends. However, this does not apply to the final explosion.",
            "Will not be stunned by a cherrybomb thrown by Ripple Friends. However, this does not apply to the final explosion. This also applies to joke rifle bullets."
        ), tags: ["Cherrybomb"]));
        GraffitiBomb = addon.Bind("GraffitiBomb", false, new ConfigurableInfo("Will not be hit by a graffiti bomb thrown by Ripple Friends.", tags: ["Graffiti Bomb"]));
        DangleFruit = addon.Bind("DangleFruit", false, new ConfigurableInfo("Will not be hit by a dangle fruit fired from a joke rifle by Ripple Friends.", tags: ["DangleFruit"]));
        JellyFish = addon.Bind("JellyFish", false, new ConfigurableInfo("Will not be caught or stunned by the tentacles of a jellyfish held by a Ripple Friend, and will not be stunned by a jellyfish thrown by Ripple Friends.", tags: ["Jellyfish"]));
        Pomegranate = addon.Bind("Pomegranate", false, new ConfigurableInfo("Will not take damage from a pomegranate dropped by Ripple Friends.", tags: ["Pomegranate"]));
        Snail = addon.Bind("Snail", false, new ConfigurableInfo("Will not be stunned by the next pop from a snail thrown, hit, or killed by a Ripple Friend.", tags: ["Snail"]));
        TubeWorm = addon.Bind("TubeWorm", false, new ConfigurableInfo("Will not be caught by a grappling worm's tongue used by Ripple Friends.", tags: ["Grappling Worm"]));
        Frog = addon.Bind("Frog", false, new ConfigurableInfo("Will not be attached by a frog thrown by Ripple Friends.", tags: ["Frog"]));
    }
}
