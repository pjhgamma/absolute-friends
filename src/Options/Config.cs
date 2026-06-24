using static RippleFriends.Options.RemixMenu;

namespace RippleFriends.Options;

internal static class Config
{
    public static Configurable<bool> FriendPlayer = Instance.config.Bind("FriendPlayer", true, new ConfigurableInfo("Includes players and slugpups as Ripple Friends."));
    public static Configurable<bool> FriendCreature = Instance.config.Bind("FriendCreature", true, new ConfigurableInfo("Includes friendly creatures, such as tamed lizards and friendly scavengers, as Ripple Friends."));
    public static Configurable<bool> FriendNeutralCreature = Instance.config.Bind("FriendNeutralCreature", false, new ConfigurableInfo("Includes ignoring creatures, such as neutralized lizards and rain deer, as Ripple Friends."));
    public static Configurable<bool> FriendIterator = Instance.config.Bind("FriendIterator", true, new ConfigurableInfo("Includes Iterators as Ripple Friends."));
    public static Configurable<bool> FriendGrabbed = Instance.config.Bind("FriendGrabbed", true, new ConfigurableInfo("Includes objects grabbed by Ripple Friends as well."));
    public static Configurable<bool> FriendArena = Instance.config.Bind("FriendArena", false, new ConfigurableInfo("Activates the Ripple Friends relationship in the Arena."));

    public static Configurable<bool> Collision = Instance.config.Bind("Collision", true, new ConfigurableInfo("Will not collide with Ripple Friends."));
    public static Configurable<bool> Weapon = Instance.config.Bind("Weapon", true, new ConfigurableInfo("Will not be hit by most weapons thrown by Ripple Friends."));
    public static Configurable<bool> Explosion = Instance.config.Bind("Explosion", true, new ConfigurableInfo("Will not be hit by most explosions caused by Ripple Friends. This also applies to chain explosions."));

    public static Configurable<bool> GrabPlayer = Instance.config.Bind("GrabPlayer", true, new ConfigurableInfo("Cannot grab a player when they enter a control input."));
    public static Configurable<float> GrabPlayerTime = Instance.config.Bind("GrabPlayerTime", 3f, new ConfigurableInfo("Sets the maximum control input time (in seconds) during which a player cannot be grabbed."));
    public static Configurable<bool> NoStealing = Instance.config.Bind("NoStealing", true, new ConfigurableInfo("Will not steal objects grabbed by Ripple Friends. If the No Stealing option is enabled in Jolly Co-op, players never steal grabbed objects from each other."));
    public static Configurable<bool> Pebbles = Instance.config.Bind("Pebbles", true, new ConfigurableInfo("Pebbles cannot kill players."));
    public static Configurable<bool> Moon = Instance.config.Bind("Moon", true, new ConfigurableInfo("Players cannot steal Moon's neurons."));
    public static Configurable<bool> Mushroom = Instance.config.Bind("Mushroom", true, new ConfigurableInfo("Share the effects of mushrooms among players."));

    public static Configurable<bool> FirecrackerPlant = Instance.config.Bind("FirecrackerPlant", true, new ConfigurableInfo("Will not be stunned by a cherrybomb thrown by Ripple Friends. However, this does not apply to the final explosion. This also applies to joke rifle bullets and chain explosions."));
    public static Configurable<bool> Bee = Instance.config.Bind("Bee", true, new ConfigurableInfo("Will not be caught by bees triggered by Ripple Friends. However, this does not apply to thoese spawned by approaching a beehive. This also applies to joke rifle bullets."));
    public static Configurable<bool> JellyFish = Instance.config.Bind("JellyFish", true, new ConfigurableInfo("Will not be caught or stunned by the tentacles of a jellyfish held by a Ripple Friends, and will not be stunned by a thrown jellyfish."));
    public static Configurable<bool> Snail = Instance.config.Bind("Snail", true, new ConfigurableInfo("Will not be stunned by the next popping from a snail thrown, hit, or killed by Ripple Friends."));
    public static Configurable<bool> TubeWorm = Instance.config.Bind("TubeWorm", true, new ConfigurableInfo("Will not be caught by the tongue of a grappling worm shot by Ripple Friends."));

    public static Configurable<bool> GourmandSlam = Instance.config.Bind("GourmandSlam", true, new ConfigurableInfo("Will not take damage from a Gourmand's roll, slide, or slam. If the Spears Miss option is enabled in Jolly Co-op, players never damage each other."));
    public static Configurable<bool> ArtificerParry = Instance.config.Bind("ArtificerParry", true, new ConfigurableInfo("Will not be stunned by a Artificer's parry. If the Spears Miss option is enabled in Jolly Co-op, players never stun each other."));
    public static Configurable<bool> SaintTongue = Instance.config.Bind("SaintTongue", true, new ConfigurableInfo("Will not be caught by the tongue shot by a Saint."));
    public static Configurable<bool> SaintAttunement = Instance.config.Bind("SaintAttunement", true, new ConfigurableInfo("Will not be instantly killed by a Saint's attunement."));

    public static Configurable<bool> FireEgg = Instance.config.Bind("FireEgg", true, new ConfigurableInfo("Will not be attached to by a fire egg thrown by Ripple Friends. However, this does not apply to the explosion."));
    public static Configurable<bool> SingularityBomb = Instance.config.Bind("SingularityBomb", true, new ConfigurableInfo("Will not be sucked or instantly killed by a singularity bomb thrown by Ripple Friends. However, this does not apply to the explosion."));

    public static Configurable<bool> Pomegranate = Instance.config.Bind("Pomegranate", true, new ConfigurableInfo("Will not take damage from a pomegranate dropped by Ripple Friends."));
    public static Configurable<bool> Frog = Instance.config.Bind("Frog", true, new ConfigurableInfo("Will not be attached to by a frog thrown by Ripple Friends."));
}
