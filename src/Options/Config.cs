namespace RippleFriends.Options;

internal static class Config
{
    private static readonly Dictionary<ConfigurableBase, List<Configurable<bool>[]>> _requirements = [];

    public static Configurable<bool> FriendSlugcat = null!;

    public static Configurable<bool> FriendCreature = null!;

    public static Configurable<bool> FriendNeutralCreature = null!;

    public static Configurable<bool> FriendIterator = null!;

    public static Configurable<bool> FriendChaining = null!;

    public static Configurable<bool> FriendGrabbed = null!;

    public static Configurable<bool> FriendGrabbedForce = null!;

    public static Configurable<bool> FriendArena = null!;

    public static Configurable<bool> Violence = null!;

    public static Configurable<bool> Collision = null!;

    public static Configurable<bool> Explosion = null!;

    public static Configurable<bool> Fear = null!;

    public static Configurable<bool> Grabbing = null!;

    public static Configurable<bool> Deaf = null!;

    public static Configurable<float> DeafRatio = null!;

    public static Configurable<bool> Blind = null!;

    public static Configurable<float> BlindRatio = null!;

    public static Configurable<bool> Hypothermia = null!;

    public static Configurable<float> HypothermiaRatio = null!;

    public static Configurable<bool> Forgiveness = null!;

    public static Configurable<float> ForgivenessRatio = null!;

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

    public static Configurable<bool> GrabbingPlayer = null!;

    public static Configurable<float> GrabbingPlayerTime = null!;

    public static Configurable<bool> Wiggle = null!;

    public static Configurable<bool> Carry = null!;

    public static Configurable<bool> Mauling = null!;

    public static Configurable<bool> GourmandSlam = null!;

    public static Configurable<bool> ArtificerParry = null!;

    public static Configurable<bool> SaintTongue = null!;

    public static Configurable<bool> SaintAttunement = null!;

    public static Configurable<bool> WatcherRipple = null!;

    public static Configurable<bool> LizardBite = null!;

    public static Configurable<bool> LizardTongue = null!;

    public static Configurable<bool> LizardSpit = null!;

    public static Configurable<bool> LizardBeam = null!;

    public static Configurable<bool> LizardBlizzard = null!;

    public static Configurable<bool> LizardPoison = null!;

    public static Configurable<bool> ScavengerShelter = null!;

    public static Configurable<bool> ScavengerTemplar = null!;

    public static Configurable<bool> Moon = null!;

    public static Configurable<bool> MoonNeuron = null!;

    public static Configurable<bool> Pebbles = null!;

    public static Configurable<bool> PebblesPearl = null!;

    public static Configurable<bool> Gate = null!;

    public static Configurable<float> GateTime = null!;

    public static Configurable<bool> GateForce = null!;

    public static Configurable<float> GateForceTime = null!;

    public static Configurable<bool> Passage = null!;

    public static Configurable<bool> TempleGuard = null!;

    public static Configurable<bool> ResonanceGate = null!;

    public static Configurable<bool> ResonanceRoom = null!;

    public static Configurable<bool> ResonanceGrab = null!;

    public static Configurable<bool> ResonanceWarp = null!;

    public static Configurable<bool> ResonanceMend = null!;

    public static Configurable<bool> ResonanceCost = null!;

    public static Configurable<float> ResonanceCostRatio = null!;

    public static Configurable<bool> ResonanceAftershock = null!;

    public static Configurable<float> ResonanceAftershockRatio = null!;

    public static Configurable<bool> ResonanceEffect = null!;

    public static Configurable<bool> FriendLink = null!;

    public static Configurable<bool> FriendName = null!;

    public static Configurable<bool> FriendIcon = null!;

    public static Configurable<bool> OwnerLink = null!;

    public static Configurable<bool> OwnerName = null!;

    public static Configurable<bool> OwnerIcon = null!;

    public static Configurable<bool> Debug = null!;

    extension(ConfigurableBase? configurableBase)
    {
        public string? Label => configurableBase?.info?.Tags is { Length: > 0 } tags ? tags[0] as string : null;

        public string? Description => configurableBase?.info?.description;

        public IEnumerable<Configurable<bool>[]> Requirements => configurableBase != null && _requirements.TryGetValue(configurableBase, out List<Configurable<bool>[]> groups) ? groups : [];

        private bool IsAllowed => configurableBase.Requirements.All(group => group.Any(master => master.IsActive));
    }

    extension(Configurable<bool>? option)
    {
        public bool IsActive => option is { Value: true } && option.IsAllowed;
    }

    public static void Bind(OptionInterface oi)
    {
        FriendSlugcat = oi.config.Bind("FriendSlugcat", true, new ConfigurableInfo(Downpour(
            "Includes players as Ripple Friends, and a creature friendly to one player counts as a Ripple Friend of every player. Ripple Friends never try to attack one another.",
            "Includes players and slugpups as Ripple Friends, and a creature friendly to one player counts as a Ripple Friend of every player. Ripple Friends never try to attack one another."
        ), tags: ["Slugcats"]));
        FriendCreature = oi.config.Bind("FriendCreature", false, new ConfigurableInfo("Includes friendly creatures, such as tamed lizards and friendly scavengers, as Ripple Friends.", tags: ["Friendly Creatures"]));
        FriendNeutralCreature = oi.config.Bind("FriendNeutralCreature", false, new ConfigurableInfo("Includes neutral creatures, such as neutralized lizards and rain deer, as Ripple Friends alongside friendly ones.", tags: ["Neutral Creatures"]));
        FriendIterator = oi.config.Bind("FriendIterator", false, new ConfigurableInfo("Includes Iterators as Ripple Friends.", tags: ["Iterators"]));

        FriendChaining = oi.config.Bind("FriendChaining", false, new ConfigurableInfo("Includes the creature Ripple Friends of the same player as one another's Ripple Friends.", tags: ["Friend Chaining"]));
        FriendGrabbed = oi.config.Bind("FriendGrabbed", true, new ConfigurableInfo("Includes objects grabbed by Ripple Friends as well.", tags: ["Grabbed Objects"]));
        FriendGrabbedForce = oi.config.Bind("FriendGrabbedForce", true, new ConfigurableInfo("Temporarily excludes a grabbed creature from Ripple Friends while the player Ripple Friend holding it is entering a grab input.", tags: ["Force Grabbing"]));
        FriendArena = oi.config.Bind("FriendArena", false, new ConfigurableInfo("Activates the Ripple Friends relationship in the Arena.", tags: ["Arena"]));

        Violence = oi.config.Bind("Violence", false, new ConfigurableInfo("Ripple Friends take no damage from one another, whatever the cause, so long as the blow can be traced back to one of them.", tags: ["Violence"]));
        Collision = oi.config.Bind("Collision", true, new ConfigurableInfo("Will not collide with Ripple Friends.", tags: ["Collisions"]));
        Explosion = oi.config.Bind("Explosion", true, new ConfigurableInfo("Will not be hit by most explosions caused by Ripple Friends.", tags: ["Explosions"]));
        Fear = oi.config.Bind("Fear", true, new ConfigurableInfo("Will not be frightened by Ripple Friends, nor by the weapons and explosives they wield.", tags: ["Fear"]));
        Grabbing = oi.config.Bind("Grabbing", true, new ConfigurableInfo(JollyCoop(
            Downpour(
                "Ripple Friends will not grab one another, nor take what they hold. Players have their own option.",
                "Ripple Friends will not grab one another, nor take what they hold. A slugpup and what it holds are exempt, and players have their own option."
            ),
            Downpour(
                "Ripple Friends will not grab one another, nor take what they hold. Players have their own option. Jolly Co-op's No Stealing option always prevents theft between players.",
                "Ripple Friends will not grab one another, nor take what they hold. A slugpup and what it holds are exempt, and players have their own option. Jolly Co-op's No Stealing option always prevents theft between players."
            )
        ), tags: ["Grabbing"]));
        Deaf = oi.config.Bind("Deaf", true, new ConfigurableInfo("Deafness gradually fades toward that of the least deafened Ripple Friend in the same room.", tags: ["Deaf"]));
        DeafRatio = oi.config.Bind("DeafRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in deafness fades away each frame while a less deafened Ripple Friend is in the same room. A value of 1 closes the difference at once, and 0 disables the additional fading."));
        Blind = oi.config.Bind("Blind", true, new ConfigurableInfo("Blindness gradually fades toward that of the least blinded Ripple Friend in the same room.", tags: ["Blind"]));
        BlindRatio = oi.config.Bind("BlindRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in blindness fades away each frame while a less blinded Ripple Friend is in the same room. A value of 1 closes the difference at once, and 0 disables the additional fading."));
        Hypothermia = oi.config.Bind("Hypothermia", true, new ConfigurableInfo("Hypothermia gradually fades toward that of the least frozen Ripple Friend in the same room.", tags: ["Hypothermia"]));
        HypothermiaRatio = oi.config.Bind("HypothermiaRatio", 0.1f, new ConfigurableInfo("Sets how much of the remaining difference in hypothermia fades away each frame while a less frozen Ripple Friend is in the same room. A value of 1 closes the difference at once, and 0 disables the warmth."));
        Forgiveness = oi.config.Bind("Forgiveness", true, new ConfigurableInfo("Attacks and thefts between Ripple Friends leave less of a grudge behind, upon the one who suffered them and upon any onlooker.", tags: ["Forgiveness"]));
        ForgivenessRatio = oi.config.Bind("ForgivenessRatio", 0.1f, new ConfigurableInfo("Sets how much of the reputation that attacks and thefts between Ripple Friends would take away is still lost. A value of 0 lets the deed pass unnoticed, and 1 leaves the reputation loss untouched."));

        Rock = oi.config.Bind("Rock", true, new ConfigurableInfo(Downpour(
            "Will not be hit by a rock thrown by Ripple Friends.",
            "Will not be hit by a rock thrown by Ripple Friends. This also applies to joke rifle bullets."
        ), tags: ["Rock"]));
        Spear = oi.config.Bind("Spear", true, new ConfigurableInfo("Will not be hit by a spear thrown by Ripple Friends.", tags: ["Spear"]));
        ExplosiveSpear = oi.config.Bind("ExplosiveSpear", true, new ConfigurableInfo("Will not be hit by an explosive spear thrown by Ripple Friends. However, this does not apply to the explosion.", tags: ["Explosive Spear"]));
        ElectricSpear = oi.config.Bind("ElectricSpear", true, new ConfigurableInfo("Will not be hit by an electric spear thrown by Ripple Friends.", tags: ["Electric Spear"]));
        HellSpear = oi.config.Bind("HellSpear", true, new ConfigurableInfo("Will not be hit by a fire spear thrown by Ripple Friends.", tags: ["Fire Spear"]));
        PoisonSpear = oi.config.Bind("PoisonSpear", true, new ConfigurableInfo("Will not be hit by a poison spear thrown by Ripple Friends.", tags: ["Poison Spear"]));
        LilyPuck = oi.config.Bind("LilyPuck", true, new ConfigurableInfo("Will not be hit by a lilypuck thrown by Ripple Friends.", tags: ["Lilypuck"]));
        ScavengerBomb = oi.config.Bind("ScavengerBomb", true, new ConfigurableInfo(Downpour(
            "Will not be hit by a grenade thrown by Ripple Friends. However, this does not apply to the explosion.",
            "Will not be hit by a grenade thrown by Ripple Friends. However, this does not apply to the explosion. This also applies to joke rifle bullets."
        ), tags: ["Grenade"]));
        SingularityBomb = oi.config.Bind("SingularityBomb", true, new ConfigurableInfo("Will not be sucked in or instantly killed by a singularity bomb thrown by Ripple Friends. However, this does not apply to the explosion. This also applies to joke rifle bullets.", tags: ["Singularity Bomb"]));
        FireEgg = oi.config.Bind("FireEgg", true, new ConfigurableInfo("Will not be attached by a fire egg thrown by Ripple Friends. However, this does not apply to the explosion. This also applies to joke rifle bullets.", tags: ["Fire Egg"]));
        SporePlant = oi.config.Bind("SporePlant", true, new ConfigurableInfo(Downpour(
            "Will not be caught by bees triggered by Ripple Friends. However, this does not apply to those spawned by approaching a beehive.",
            "Will not be caught by bees triggered by Ripple Friends. However, this does not apply to those spawned by approaching a beehive. This also applies to joke rifle bullets."
        ), tags: ["Beehive"]));
        Boomerang = oi.config.Bind("Boomerang", true, new ConfigurableInfo("Will not be hit by a boomerang thrown by Ripple Friends.", tags: ["Boomerang"]));
        Mushroom = oi.config.Bind("Mushroom", true, new ConfigurableInfo("Will share the effects of mushrooms with slugcat Ripple Friends.", tags: ["Mushroom"]));
        FlareBomb = oi.config.Bind("FlareBomb", true, new ConfigurableInfo(Downpour(
            "Will not be hit by a flash bang thrown by Ripple Friends.",
            "Will not be hit by a flash bang thrown by Ripple Friends. This also applies to joke rifle bullets."
        ), tags: ["Flash Bang"]));
        PuffBall = oi.config.Bind("PuffBall", true, new ConfigurableInfo(Downpour(
            "Will not be hit by a spore puff thrown by Ripple Friends.",
            "Will not be hit by a spore puff thrown by Ripple Friends. This also applies to joke rifle bullets."
        ), tags: ["Spore Puff"]));
        WaterNut = oi.config.Bind("WaterNut", true, new ConfigurableInfo("Will not be hit by an unswollen bubble fruit thrown by Ripple Friends.", tags: ["Bubble Fruit"]));
        FirecrackerPlant = oi.config.Bind("FirecrackerPlant", true, new ConfigurableInfo(Downpour(
            "Will not be stunned by a cherrybomb thrown by Ripple Friends. However, this does not apply to the final explosion.",
            "Will not be stunned by a cherrybomb thrown by Ripple Friends. However, this does not apply to the final explosion. This also applies to joke rifle bullets."
        ), tags: ["Cherrybomb"]));
        GraffitiBomb = oi.config.Bind("GraffitiBomb", true, new ConfigurableInfo("Will not be hit by a graffiti bomb thrown by Ripple Friends.", tags: ["Graffiti Bomb"]));
        DangleFruit = oi.config.Bind("DangleFruit", true, new ConfigurableInfo("Will not be hit by a dangle fruit fired from a joke rifle by Ripple Friends.", tags: ["DangleFruit"]));
        JellyFish = oi.config.Bind("JellyFish", true, new ConfigurableInfo("Will not be caught or stunned by the tentacles of a jellyfish held by a Ripple Friend, and will not be stunned by a jellyfish thrown by Ripple Friends.", tags: ["Jellyfish"]));
        Pomegranate = oi.config.Bind("Pomegranate", true, new ConfigurableInfo("Will not take damage from a pomegranate dropped by Ripple Friends.", tags: ["Pomegranate"]));
        Snail = oi.config.Bind("Snail", true, new ConfigurableInfo("Will not be stunned by the next pop from a snail thrown, hit, or killed by a Ripple Friend.", tags: ["Snail"]));
        TubeWorm = oi.config.Bind("TubeWorm", true, new ConfigurableInfo("Will not be caught by a grappling worm's tongue used by Ripple Friends.", tags: ["Grappling Worm"]));
        Frog = oi.config.Bind("Frog", true, new ConfigurableInfo("Will not be attached by a frog thrown by Ripple Friends.", tags: ["Frog"]));

        GrabbingPlayer = oi.config.Bind("GrabbingPlayer", true, new ConfigurableInfo("Will not be grabbed by player Ripple Friends while entering control inputs.", tags: ["Grab Player"]));
        GrabbingPlayerTime = oi.config.Bind("GrabbingPlayerTime", 1f, new ConfigurableInfo("Sets the maximum control input time (in seconds) during which a player Ripple Friend cannot be grabbed."));
        Wiggle = oi.config.Bind("Wiggle", true, new ConfigurableInfo("Can wiggle free from the grasp of player Ripple Friends.", tags: ["Wiggle"]));
        Carry = oi.config.Bind("Carry", true, new ConfigurableInfo("Can pick up and carry tracked Ripple Friends that are otherwise too large to be held. A carried Ripple Friend rests still, and stirs again once it is let go.", tags: ["Carry"]));
        Mauling = oi.config.Bind("Mauling", true, new ConfigurableInfo(Downpour(
            "Will not eat Ripple Friends.",
            "Will not maul or eat Ripple Friends."
        ), tags: ["Mauling"]));
        GourmandSlam = oi.config.Bind("GourmandSlam", true, new ConfigurableInfo(JollyCoop(
            "Will not take damage from the roll, slide, or slam of a Gourmand Ripple Friend.",
            "Will not take damage from the roll, slide, or slam of a Gourmand Ripple Friend. If the Spears Miss option is enabled in Jolly Co-op, slugcats will never damage each other."
        ), tags: ["Gourmand Slam"]));
        ArtificerParry = oi.config.Bind("ArtificerParry", true, new ConfigurableInfo(JollyCoop(
            "Will not be stunned by the parry of an Artificer Ripple Friend.",
            "Will not be stunned by the parry of an Artificer Ripple Friend. If the Spears Miss option is enabled in Jolly Co-op, slugcats will never stun each other."
        ), tags: ["Artificer Parry"]));
        SaintTongue = oi.config.Bind("SaintTongue", true, new ConfigurableInfo("Will not be caught by the tongue of a Saint Ripple Friend.", tags: ["Saint Tongue"]));
        SaintAttunement = oi.config.Bind("SaintAttunement", true, new ConfigurableInfo("Will not be instantly killed by the attunement of a Saint Ripple Friend.", tags: ["Saint Attunement"]));
        WatcherRipple = oi.config.Bind("WatcherRipple", true, new ConfigurableInfo("Will share camouflage and its gauge with Watcher Ripple Friends, yet what the game forces off is not shared, and synchronized later. Tracked Ripple Friends also stay in their player's ripple space and in sight.", tags: ["Watcher Ripple"]));
        LizardBite = oi.config.Bind("LizardBite", false, new ConfigurableInfo("Will not be bitten by lizard Ripple Friends.", tags: ["Lizard Bite"]));
        LizardTongue = oi.config.Bind("LizardTongue", false, new ConfigurableInfo("Will not be caught by the tongue of a lizard Ripple Friend. Instead, an indigo lizard Ripple Friend licks any Ripple Friend caught in a locust swarm, not only the one it follows.", tags: ["Lizard Tongue"]));
        LizardSpit = oi.config.Bind("LizardSpit", false, new ConfigurableInfo("Will not be stuck by the spit of a lizard Ripple Friend.", tags: ["Lizard Spit"]));
        LizardBeam = oi.config.Bind("LizardBeam", false, new ConfigurableInfo("Will not be struck by the laser of a blizzard lizard Ripple Friend.", tags: ["Lizard Laser"]));
        LizardBlizzard = oi.config.Bind("LizardBlizzard", false, new ConfigurableInfo("Will not be swept up by the shield around a blizzard lizard Ripple Friend.", tags: ["Lizard Shield"]));
        LizardPoison = oi.config.Bind("LizardPoison", false, new ConfigurableInfo("Will not be poisoned by the touch of a basilisk lizard Ripple Friend, nor dazed by the mushroom haze around it.", tags: ["Lizard Poison"]));

        ScavengerShelter = oi.config.Bind("ScavengerShelter", false, new ConfigurableInfo("Will not have objects inside a shelter taken by scavenger Ripple Friends.", tags: ["Scavenger Shelter"]));
        ScavengerTemplar = oi.config.Bind("ScavengerTemplar", false, new ConfigurableInfo("Will not be struck by the shockwave from the karma shield of a templar Ripple Friend.", tags: ["Scavenger Shield"]));

        Moon = oi.config.Bind("Moon", false, new ConfigurableInfo("Will not lower the opinion of Moon Ripple Friend, nor make her refuse to speak.", tags: ["Moon"]));
        MoonNeuron = oi.config.Bind("MoonNeuron", false, new ConfigurableInfo("Will not steal the neurons of Moon Ripple Friend.", tags: ["Moon Neuron"]));
        Pebbles = oi.config.Bind("Pebbles", false, new ConfigurableInfo("Will not be killed by Pebbles Ripple Friend.", tags: ["Pebbles"]));
        PebblesPearl = oi.config.Bind("PebblesPearl", false, new ConfigurableInfo("Will not steal the pearl of Pebbles Ripple Friend.", tags: ["Pebbles Pearl"]));

        Gate = oi.config.Bind("Gate", true, new ConfigurableInfo(Watcher(
            "Shelters and karma gates wait for tracked Ripple Friends and will not activate while any player is entering control inputs.",
            "Shelters, karma gates, and warp points wait for tracked Ripple Friends and will not activate while any player is entering control inputs."
        ), tags: ["Gate Delay"]));
        GateTime = oi.config.Bind("GateTime", 1f, new ConfigurableInfo(Watcher(
            "Sets the minimum time (in seconds) of no control input from the player required for shelters and karma gates to activate.",
            "Sets the minimum time (in seconds) of no control input from the player required for shelters, karma gates, and warp points to activate."
        )));
        GateForce = oi.config.Bind("GateForce", true, new ConfigurableInfo(Watcher(
            "Shelters and karma gates forcefully activate, ignoring non-player Ripple Friends.",
            "Shelters, karma gates, and warp points forcefully activate, ignoring non-player Ripple Friends."
        ), tags: ["Gate Force"]));
        GateForceTime = oi.config.Bind("GateForceTime", 3f, new ConfigurableInfo(Watcher(
            "Sets how much longer (in seconds) the player must go without control input for shelters and karma gates to forcefully activate, ignoring non-player Ripple Friends.",
            "Sets how much longer (in seconds) the player must go without control input for shelters, karma gates, and warp points to forcefully activate, ignoring non-player Ripple Friends."
        )));
        Passage = oi.config.Bind("Passage", true, new ConfigurableInfo("Tracked Ripple Friends that survive the cycle travel with the player from anywhere in the world, even through a passage.", tags: ["Passages"]));
        TempleGuard = oi.config.Bind("TempleGuard", true, new ConfigurableInfo("Will not draw a guardian's attention, nor be moved by its telekinesis, while a player Ripple Friend holds the karma required to pass.", tags: ["Guardian"]));

        ResonanceGate = oi.config.Bind("ResonanceGate", false, new ConfigurableInfo(Watcher(
            "Automatically resonates as a shelter or a karma gate closes, or as a karma room activates.",
            "Automatically resonates as a shelter or a karma gate closes, or as a karma room or a warp point activates."
        ), tags: ["Gate Resonance"]));
        ResonanceRoom = oi.config.Bind("ResonanceRoom", false, new ConfigurableInfo("Resonates by holding the jump key while every player Ripple Friend gathers in one room, reaching only the Ripple Friends in that room, or every tracked one when they gather at a gate and Gate Resonance is enabled.", tags: ["Room Resonance"]));
        ResonanceGrab = oi.config.Bind("ResonanceGrab", false, new ConfigurableInfo("Resonates by holding the jump key while a player Ripple Friend grabs a hurt or dead Ripple Friend, mending only the one in hand.", tags: ["Grab Resonance"]));
        ResonanceWarp = oi.config.Bind("ResonanceWarp", false, new ConfigurableInfo("Warps eligible tracked Ripple Friends to the player's location upon resonance.", tags: ["Warp"]));
        ResonanceMend = oi.config.Bind("ResonanceMend", false, new ConfigurableInfo("Mends eligible tracked Ripple Friends upon resonance, closing their wounds and drawing out poison and cold, and bringing back the ones that have died.", tags: ["Mend"]));
        ResonanceCost = oi.config.Bind("ResonanceCost", true, new ConfigurableInfo("A resonance demands a cost, which grows with the distance each Ripple Friend is called from and the time it has spent dead, and holds everyone who resonated still and out of breath.", tags: ["Cost"]));
        ResonanceCostRatio = oi.config.Bind("ResonanceCostRatio", 1f, new ConfigurableInfo("Sets how much of the resonance cost is demanded. A value of 1 demands it in full, and 0 makes a resonance instant and free."));
        ResonanceAftershock = oi.config.Bind("ResonanceAftershock", true, new ConfigurableInfo("The paid resonance cost lingers as an aftershock, making everyone who resonated stumble now and then until the cycle ends.", tags: ["Aftershock"]));
        ResonanceAftershockRatio = oi.config.Bind("ResonanceAftershockRatio", 0.1f, new ConfigurableInfo("Sets how much of the paid resonance cost lingers as an aftershock. A value of 1 leaves all of it behind, and 0 leaves nothing."));
        ResonanceEffect = oi.config.Bind("ResonanceEffect", false, new ConfigurableInfo("Displays visual effects upon resonance. A gate resonance melts the room into the void, while a room or grab resonance answers with ripples.", tags: ["Visual Effects"]));

        FriendLink = oi.config.Bind("FriendLink", false, new ConfigurableInfo("Draws a line between two things as the mod treats them as Ripple Friends.", tags: ["Friend Link"]));
        FriendName = oi.config.Bind("FriendName", false, new ConfigurableInfo("Draws the name above every Ripple Friend in view, and keeps a tracked friend's name at the edge of the screen while it is out of view.", tags: ["Friend Name"]));
        FriendIcon = oi.config.Bind("FriendIcon", false, new ConfigurableInfo("Draws an icon above every Ripple Friend in view, and keeps a tracked friend's icon at the edge of the screen while it is out of view.", tags: ["Friend Icon"]));
        OwnerLink = oi.config.Bind("OwnerLink", false, new ConfigurableInfo("Draws a line between each object and its owning creature, thickest at the owner's end.", tags: ["Owner Link"]));
        OwnerName = oi.config.Bind("OwnerName", false, new ConfigurableInfo("Draws the owner's name above everything in view that has an owner.", tags: ["Owner Name"]));
        OwnerIcon = oi.config.Bind("OwnerIcon", false, new ConfigurableInfo("Draws the owner's icon above everything in view that has an owner.", tags: ["Owner Icon"]));

        Debug = oi.config.Bind("Debug", false, new ConfigurableInfo("Logs where the mod patches landed and what they are doing.", tags: ["Debug"]));

        Relate();
    }

    private static void Relate()
    {
        _requirements.Clear();

        Require(FriendCreature, FriendSlugcat);
        Require(FriendNeutralCreature, FriendCreature);

        RequireAny(FriendChaining, FriendCreature, FriendIterator);
        Require(FriendGrabbedForce, FriendSlugcat, FriendGrabbed);

        Require(DeafRatio, Deaf);
        Require(BlindRatio, Blind);
        Require(HypothermiaRatio, Hypothermia);
        Require(ForgivenessRatio, Forgiveness);

        Require(Mushroom, FriendSlugcat);

        Require(GrabbingPlayer, FriendSlugcat);
        Require(GrabbingPlayerTime, GrabbingPlayer);
        Require(Wiggle, FriendSlugcat);
        Require(Carry, FriendSlugcat);
        Require(Mauling, FriendSlugcat);
        Require(GourmandSlam, FriendSlugcat);
        Require(ArtificerParry, FriendSlugcat);
        Require(SaintTongue, FriendSlugcat);
        Require(SaintAttunement, FriendSlugcat);
        Require(WatcherRipple, FriendSlugcat);

        Require(LizardBite, FriendCreature);
        Require(LizardTongue, FriendCreature);
        Require(LizardSpit, FriendCreature);
        Require(LizardBeam, FriendCreature);
        Require(LizardBlizzard, FriendCreature);
        Require(LizardPoison, FriendCreature);

        Require(ScavengerShelter, FriendCreature);
        Require(ScavengerTemplar, FriendCreature);

        Require(Moon, FriendIterator);
        Require(MoonNeuron, FriendIterator);
        Require(Pebbles, FriendIterator);
        Require(PebblesPearl, FriendIterator);

        Require(GateTime, Gate);
        Require(GateForce, FriendSlugcat, Gate);
        Require(GateForceTime, GateForce);

        Require(Passage, FriendSlugcat);
        Require(TempleGuard, FriendSlugcat);

        Require(ResonanceGate, FriendSlugcat);
        Require(ResonanceRoom, FriendSlugcat);
        Require(ResonanceGrab, FriendSlugcat);

        RequireAny(ResonanceWarp, ResonanceGate, ResonanceRoom);
        RequireAny(ResonanceMend, ResonanceGate, ResonanceRoom, ResonanceGrab);
        RequireAny(ResonanceCost, ResonanceWarp, ResonanceMend);
        Require(ResonanceCostRatio, ResonanceCost);
        Require(ResonanceAftershock, ResonanceCost);
        Require(ResonanceAftershockRatio, ResonanceAftershock);
        RequireAny(ResonanceEffect, ResonanceWarp, ResonanceMend);
    }

    private static void Require(ConfigurableBase target, params Configurable<bool>[] masters)
    {
        foreach (var master in masters)
        {
            RequireAny(target, master);
        }
    }

    private static void RequireAny(ConfigurableBase target, params Configurable<bool>[] masters)
    {
        if (!_requirements.TryGetValue(target, out List<Configurable<bool>[]> groups))
        {
            _requirements[target] = groups = [];
        }

        groups.Add(masters);
    }

    private static string JollyCoop(string text, string jollyCoopText)
    {
        return ModManager.JollyCoop ? jollyCoopText : text;
    }

    private static string Downpour(string text, string downpourText)
    {
        return ModManager.MSC ? downpourText : text;
    }

    private static string Watcher(string text, string watcherText)
    {
        return ModManager.Watcher ? watcherText : text;
    }
}
