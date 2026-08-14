namespace RippleFriends.Options;

internal static class ConfigurableExtensions
{
    public static string? Label(this ConfigurableBase? configurable) => configurable?.info?.Tags is { Length: > 0 } tags ? tags[0] as string : null;
}

internal static class Config
{
    public static Configurable<bool> FriendPlayer = null!;

    public static Configurable<bool> FriendCreature = null!;

    public static Configurable<bool> FriendNeutralCreature = null!;

    public static Configurable<bool> FriendIterator = null!;

    public static Configurable<bool> FriendGrabbed = null!;

    public static Configurable<bool> FriendArena = null!;

    public static Configurable<bool> Collision = null!;

    public static Configurable<bool> Weapon = null!;

    public static Configurable<bool> Explosion = null!;

    public static Configurable<bool> GrabPlayer = null!;

    public static Configurable<float> GrabPlayerTime = null!;

    public static Configurable<bool> NoStealing = null!;

    public static Configurable<bool> Pebbles = null!;

    public static Configurable<bool> Moon = null!;

    public static Configurable<bool> Mushroom = null!;

    public static Configurable<bool> FirecrackerPlant = null!;

    public static Configurable<bool> Bee = null!;

    public static Configurable<bool> JellyFish = null!;

    public static Configurable<bool> Snail = null!;

    public static Configurable<bool> TubeWorm = null!;

    public static Configurable<bool> GourmandSlam = null!;

    public static Configurable<bool> ArtificerParry = null!;

    public static Configurable<bool> SaintTongue = null!;

    public static Configurable<bool> SaintAttunement = null!;

    public static Configurable<bool> FireEgg = null!;

    public static Configurable<bool> SingularityBomb = null!;

    public static Configurable<bool> Pomegranate = null!;

    public static Configurable<bool> Frog = null!;

    public static Configurable<bool> FriendLink = null!;

    public static Configurable<bool> OwnerLink = null!;

    public static Configurable<bool> OwnerName = null!;

    public static Configurable<bool> Debug = null!;

    public static void Bind(OptionInterface oi)
    {
        FriendPlayer = oi.config.Bind("FriendPlayer", true, new ConfigurableInfo("Includes players and slugpups as Ripple Friends.", tags: ["Player"]));
        FriendCreature = oi.config.Bind("FriendCreature", true, new ConfigurableInfo("Includes friendly creatures, such as tamed lizards and friendly scavengers, as Ripple Friends.", tags: ["Friendly Creatures"]));
        FriendNeutralCreature = oi.config.Bind("FriendNeutralCreature", false, new ConfigurableInfo("Includes ignoring creatures, such as neutralized lizards and rain deer, as Ripple Friends.", tags: ["Neutral Creatures"]));
        FriendIterator = oi.config.Bind("FriendIterator", true, new ConfigurableInfo("Includes Iterators as Ripple Friends.", tags: ["Iterators"]));
        FriendGrabbed = oi.config.Bind("FriendGrabbed", true, new ConfigurableInfo("Includes objects grabbed by Ripple Friends as well.", tags: ["Grabbed Objects"]));
        FriendArena = oi.config.Bind("FriendArena", false, new ConfigurableInfo("Activates the Ripple Friends relationship in the Arena.", tags: ["Arena"]));

        Collision = oi.config.Bind("Collision", true, new ConfigurableInfo("Will not collide with Ripple Friends.", tags: ["Collisions"]));
        Weapon = oi.config.Bind("Weapon", true, new ConfigurableInfo("Will not be hit by most weapons thrown by Ripple Friends.", tags: ["Weapons"]));
        Explosion = oi.config.Bind("Explosion", true, new ConfigurableInfo("Will not be hit by most explosions caused by Ripple Friends. This also applies to chain explosions.", tags: ["Explosions"]));

        GrabPlayer = oi.config.Bind("GrabPlayer", true, new ConfigurableInfo("Cannot grab a player when they enter a control input.", tags: ["Grab Player"]));
        GrabPlayerTime = oi.config.Bind("GrabPlayerTime", 3f, new ConfigurableInfo("Sets the maximum control input time (in seconds) during which a player cannot be grabbed."));
        NoStealing = oi.config.Bind("NoStealing", true, new ConfigurableInfo("Will not steal objects grabbed by Ripple Friends. If the No Stealing option is enabled in Jolly Co-op, players never steal grabbed objects from each other.", tags: ["No Stealing"]));
        Pebbles = oi.config.Bind("Pebbles", true, new ConfigurableInfo("Pebbles cannot kill players.", tags: ["Pebbles"]));
        Moon = oi.config.Bind("Moon", true, new ConfigurableInfo("Players cannot steal Moon's neurons.", tags: ["Moon"]));
        Mushroom = oi.config.Bind("Mushroom", true, new ConfigurableInfo("Share the effects of mushrooms among players.", tags: ["Mushroom"]));

        FirecrackerPlant = oi.config.Bind("FirecrackerPlant", true, new ConfigurableInfo("Will not be stunned by a cherrybomb thrown by Ripple Friends. However, this does not apply to the final explosion. This also applies to joke rifle bullets and chain explosions.", tags: ["Cherrybomb"]));
        Bee = oi.config.Bind("Bee", true, new ConfigurableInfo("Will not be caught by bees triggered by Ripple Friends. However, this does not apply to thoese spawned by approaching a beehive. This also applies to joke rifle bullets.", tags: ["Beehive"]));
        JellyFish = oi.config.Bind("JellyFish", true, new ConfigurableInfo("Will not be caught or stunned by the tentacles of a jellyfish held by a Ripple Friends, and will not be stunned by a thrown jellyfish.", tags: ["Jellyfish"]));
        Snail = oi.config.Bind("Snail", true, new ConfigurableInfo("Will not be stunned by the next popping from a snail thrown, hit, or killed by Ripple Friends.", tags: ["Snail"]));
        TubeWorm = oi.config.Bind("TubeWorm", true, new ConfigurableInfo("Will not be caught by the tongue of a grappling worm shot by Ripple Friends.", tags: ["Grappling Worm"]));

        GourmandSlam = oi.config.Bind("GourmandSlam", true, new ConfigurableInfo("Will not take damage from a Gourmand's roll, slide, or slam. If the Spears Miss option is enabled in Jolly Co-op, players never damage each other.", tags: ["Gourmand Slam"]));
        ArtificerParry = oi.config.Bind("ArtificerParry", true, new ConfigurableInfo("Will not be stunned by a Artificer's parry. If the Spears Miss option is enabled in Jolly Co-op, players never stun each other.", tags: ["Artificer Parry"]));
        SaintTongue = oi.config.Bind("SaintTongue", true, new ConfigurableInfo("Will not be caught by the tongue shot by a Saint.", tags: ["Saint Tongue"]));
        SaintAttunement = oi.config.Bind("SaintAttunement", true, new ConfigurableInfo("Will not be instantly killed by a Saint's attunement.", tags: ["Saint Attunement"]));

        FireEgg = oi.config.Bind("FireEgg", true, new ConfigurableInfo("Will not be attached to by a fire egg thrown by Ripple Friends. However, this does not apply to the explosion.", tags: ["Fire Egg"]));
        SingularityBomb = oi.config.Bind("SingularityBomb", true, new ConfigurableInfo("Will not be sucked or instantly killed by a singularity bomb thrown by Ripple Friends. However, this does not apply to the explosion.", tags: ["Singularity Bomb"]));

        Pomegranate = oi.config.Bind("Pomegranate", true, new ConfigurableInfo("Will not take damage from a pomegranate dropped by Ripple Friends.", tags: ["Pomegranate"]));
        Frog = oi.config.Bind("Frog", true, new ConfigurableInfo("Will not be attached to by a frog thrown by Ripple Friends.", tags: ["Frog"]));

        FriendLink = oi.config.Bind("FriendLink", false, new ConfigurableInfo("Draws a line between two Ripple Friend objects.", tags: ["Friend Link"]));
        OwnerLink = oi.config.Bind("OwnerLink", false, new ConfigurableInfo("Draws a line between each object and its owning creature, thickest at the owner's end.", tags: ["Owner Link"]));
        OwnerName = oi.config.Bind("OwnerName", false, new ConfigurableInfo("Draws the owning creature's name tag above each object.", tags: ["Owner Name"]));

        Debug = oi.config.Bind("Debug", false, new ConfigurableInfo("Logs where the mod patches landed and what they are doing.", tags: ["Debug"]));
    }
}
