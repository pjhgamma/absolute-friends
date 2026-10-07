using System.Globalization;
using System.Runtime.CompilerServices;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.FriendshipEditor;

internal static class RuleEffects
{
    internal const string GlobalRegion = "EVERY";

    private const string FriendPrefix = "#";

    private static readonly ConditionalWeakTable<SocialMemory.Relationship, Owner> _owners = new();

    private static readonly ConditionalWeakTable<AbstractCreature, string> _friendKeys = new();

    internal static bool ReputationActive => Config.ReputationEditor.IsActive;

    internal static bool LikeActive => Config.LikeEditor.IsActive;

    private static string RegionKey(RainWorldGame game, int region)
    {
        string[] names = game.rainWorld.progression.regionNames;

        return region >= 0 && region < names.Length ? names[region] : GlobalRegion;
    }

    private static string PlayerKey(int playerNumber) => playerNumber.ToString(CultureInfo.InvariantCulture);

    private static string FriendKey(AbstractCreature friend) => _friendKeys.GetValue(friend, creature => FriendPrefix + creature.ID);

    internal static RuleKey ReputationKey(RainWorldGame game, string subject, string region, int playerNumber) => new(
        subject,
        game.IsArenaSession ? Rules.All : region,
        game.IsStorySession ? Rules.All : PlayerKey(playerNumber),
        Stat.Reputation);

    internal static string TargetKey(AbstractCreature target) => target is { IsPlayer: true, state: PlayerState playerState }
        ? PlayerKey(playerState.playerNumber)
        : FriendKey(target);

    internal static RuleKey TypeKey(string type, AbstractCreature target, Stat stat) => new(type, Rules.All, TargetKey(target), stat);

    internal static Rule? CreatureRule(AbstractCreature creature, AbstractCreature? target, Stat stat)
    {
        if (target == null || creature.world?.game is not { } game)
        {
            return null;
        }

        Rules rules = RuleStore.For(game);
        string key = TargetKey(target);

        return target.IsPlayer || rules.HasTarget(key) ? rules.Resolve(new RuleKey(creature.creatureTemplate.type.value, Rules.All, key, stat)) : null;
    }

    internal static Dictionary<EntityID, AbstractCreature> LoadedCreatures(RainWorldGame game)
    {
        Dictionary<EntityID, AbstractCreature> creatures = [];

        foreach (AbstractRoom room in game.world?.abstractRooms ?? [])
        {
            foreach (AbstractCreature creature in room.creatures)
            {
                creatures[creature.ID] = creature;
            }

            foreach (AbstractWorldEntity entity in room.entitiesInDens)
            {
                if (entity is AbstractCreature creature)
                {
                    creatures[creature.ID] = creature;
                }
            }
        }

        foreach (AbstractCreature player in game.Players)
        {
            creatures[player.ID] = player;
        }

        return creatures;
    }

    internal static void ApplyReputation(RainWorldGame game)
    {
        if (ReputationActive && RuleStore.For(game).HasReputationRules && game.session?.creatureCommunities is { } communities)
        {
            ApplyReputation(game, communities);
        }
    }

    internal static void ApplyLikes(RainWorldGame game)
    {
        Rules rules = RuleStore.For(game);

        if (!LikeActive || !rules.HasLikeRules || game.world == null)
        {
            return;
        }

        List<AbstractCreature> creatures = [.. LoadedCreatures(game).Values];
        List<AbstractCreature> targets = [.. Targets(game, rules, creatures)];

        foreach (AbstractCreature creature in creatures)
        {
            ApplyLikes(creature, targets);
        }
    }

    internal static void ApplyReputation(RainWorldGame game, CreatureCommunities communities)
    {
        for (int community = 0; community < communities.playerOpinions.GetLength(0); community++)
        {
            for (int region = 0; region < communities.playerOpinions.GetLength(1); region++)
            {
                for (int player = 0; player < communities.playerOpinions.GetLength(2); player++)
                {
                    ApplyReputation(game, communities, community, region, player);
                }
            }
        }
    }

    internal static void ApplyReputation(RainWorldGame game, CreatureCommunities communities, int community, int region, int player)
    {
        if (!ReputationActive || ExtEnum<CreatureCommunities.CommunityID>.values.GetEntry(community + 1) is not { } name)
        {
            return;
        }

        RuleKey key = ReputationKey(game, name, RegionKey(game, region - 1), player);

        if (RuleStore.For(game).Resolve(key) is not { Enforces: true } rule)
        {
            return;
        }

        float value = communities.playerOpinions[community, region, player];
        float ruled = rule.Apply(value);

        if (ruled != value)
        {
            communities.playerOpinions[community, region, player] = ruled;
            communities.loadedPlayerOpinions[community, region, player] = ruled;
        }
    }

    internal static void ApplyLikes(AbstractCreature creature, IEnumerable<AbstractCreature> targets)
    {
        if (creature.IsPlayer || creature.state is not { } state)
        {
            return;
        }

        foreach (AbstractCreature target in targets)
        {
            if (target == creature)
            {
                continue;
            }

            LikeRules rules = new(creature, target);

            if (!rules.Enforces)
            {
                continue;
            }

            state.socialMemory ??= new();

            SocialMemory.Relationship relationship = state.socialMemory.GetOrInitiateRelationship(target.ID);

            _owners.GetValue(relationship, _ => new(creature, target));
            rules.Apply(relationship);
        }
    }

    private static IEnumerable<AbstractCreature> Targets(RainWorldGame game, Rules rules, IEnumerable<AbstractCreature> creatures) =>
        game.Players.Concat(creatures.Where(creature => !creature.IsPlayer && rules.HasTarget(FriendKey(creature))));

    internal static void ApplyRelationship(SocialMemory.Relationship relationship)
    {
        if (LikeActive && _owners.TryGetValue(relationship, out Owner owner))
        {
            new LikeRules(owner.Creature, owner.Target).Apply(relationship);
        }
    }

    private readonly struct LikeRules(AbstractCreature creature, AbstractCreature? target)
    {
        private readonly Rule? _like = CreatureRule(creature, target, Stat.Like);

        private readonly Rule? _tempLike = CreatureRule(creature, target, Stat.TempLike);

        private readonly Rule? _know = CreatureRule(creature, target, Stat.Know);

        internal bool Enforces => _like is { Enforces: true } || _tempLike is { Enforces: true } || _know is { Enforces: true };

        internal void Apply(SocialMemory.Relationship relationship)
        {
            relationship.like = _like?.Apply(relationship.like) ?? relationship.like;
            relationship.tempLike = _tempLike?.Apply(relationship.tempLike) ?? relationship.tempLike;
            relationship.know = _know?.Apply(relationship.know) ?? relationship.know;
        }
    }

    private sealed class Owner(AbstractCreature creature, AbstractCreature? target)
    {
        internal AbstractCreature Creature { get; } = creature;

        internal AbstractCreature? Target { get; } = target;
    }
}
