using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using System.Runtime.CompilerServices;

namespace AbsoluteFriends.Core;

public static class FriendUtils
{
    public const float FriendLikeThreshold = 0.5f;

    public const float FriendReputationThreshold = 0f;

    private static readonly AddonLogger _logger = Reporter.GetLogger(Plugin.Name);

    private static readonly HashSet<AbstractCreature> _friendSet = [];

    private static readonly List<Func<AbstractCreature, AbstractCreature, bool?>> _friendshipRules = [];

    private static readonly ConditionalWeakTable<FriendTracker, SocialMemory.Relationship> _adoptedFriendRelationships = new();

    public static bool IsFriendSession => Config.FriendArena.IsActive || RainWorldUtils.CurrentGame?.IsArenaSession == false;

    public static IEnumerable<AbstractCreature> TrackedFriends
    {
        get
        {
            if (!Config.FriendSlugcat.IsActive || !IsFriendSession)
            {
                yield break;
            }

            foreach (var abstractCreature in _friendSet)
            {
                if (abstractCreature.FriendshipRuleResult ?? (abstractCreature.IsSlugcat || Config.FriendCreature.IsActive))
                {
                    yield return abstractCreature;
                }
            }
        }
    }

    public static IEnumerable<AbstractCreature> TrackedFriendsIncludingPlayers
    {
        get
        {
            if (Config.FriendSlugcat.IsActive && IsFriendSession)
            {
                foreach (var abstractPlayer in Players)
                {
                    yield return abstractPlayer;
                }
            }

            foreach (var abstractCreature in TrackedFriends)
            {
                yield return abstractCreature;
            }
        }
    }

    public static IEnumerable<AbstractCreature> TrackedSlugcats
    {
        get
        {
            if (!IsFriendSession)
            {
                yield break;
            }

            foreach (var abstractPlayer in Players)
            {
                yield return abstractPlayer;
            }

            foreach (var abstractCreature in _friendSet)
            {
                if (abstractCreature.IsSlugcat)
                {
                    yield return abstractCreature;
                }
            }
        }
    }

    private static IEnumerable<AbstractCreature> Players => RainWorldUtils.CurrentGame?.Players ?? [];

    public static void RegisterFriendshipRule(Func<AbstractCreature, AbstractCreature, bool?> rule)
    {
        if (rule != null && !_friendshipRules.Contains(rule))
        {
            _friendshipRules.Add(rule);
        }
    }

    public static void UnregisterFriendshipRule(Func<AbstractCreature, AbstractCreature, bool?> rule) => _friendshipRules.Remove(rule);

    private static bool TryGetFriendLikes(SocialMemory socialMemory, EntityID subjectID, out float like, out float tempLike)
    {
        like = socialMemory.GetLike(subjectID);
        tempLike = socialMemory.GetTempLike(subjectID);

        if (tempLike == 0f)
        {
            tempLike = like;
        }

        return like > FriendLikeThreshold && tempLike > FriendLikeThreshold;
    }

    extension(AbstractCreature? source)
    {
        public void Track()
        {
            if (source is { } abstractCreature and not AbstractOwner)
            {
                _friendSet.Add(abstractCreature);
            }
        }

        public void Untrack()
        {
            if (source is { } abstractCreature)
            {
                _friendSet.Remove(abstractCreature);
            }
        }

        public bool Likes(AbstractCreature? target)
        {
            if (source?.state?.socialMemory is not { } socialMemory || target == null)
            {
                return false;
            }

            return TryGetFriendLikes(socialMemory, target.ID, out _, out _);
        }

        private float? GetReputation(AbstractCreature? target)
        {
            if (
                source?.creatureTemplate?.communityID is not { } communityID
                || communityID == CreatureCommunities.CommunityID.None
                || target?.state is not PlayerState playerState
                || source.world?.game?.session?.creatureCommunities is not { } communities
            )
            {
                return null;
            }

            return communities.LikeOfPlayer(communityID, source.world.RegionNumber, playerState.playerNumber);
        }

        private bool IsFriendlyLizard => ModManager.CoopAvailable && Custom.rainWorld?.options?.friendlyLizards == true && source?.creatureTemplate?.IsLizard == true && (source.abstractAI?.RealAI?.friendTracker).HasSlugcatFriend;

        private bool IsScavengerArtificerPair(AbstractCreature abstractSlugcat)
        {
            return source?.abstractAI?.RealAI is ScavengerAI
                && ModManager.MSC
                && abstractSlugcat.state is PlayerState { slugcatCharacter: var slugcatCharacter }
                && slugcatCharacter == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        private bool? EvaluateFriendshipRules(AbstractCreature abstractSlugcat)
        {
            if (source is not { } subject)
            {
                return null;
            }

            bool? granted = null;

            for (int index = 0; index < _friendshipRules.Count; index++)
            {
                try
                {
                    if (_friendshipRules[index](subject, abstractSlugcat) is not { } verdict)
                    {
                        continue;
                    }

                    if (!verdict)
                    {
                        return false;
                    }

                    granted = true;
                }
                catch (Exception exception)
                {
                    _logger.LogError("A friendship rule threw an exception and was disabled", exception);

                    _friendshipRules.RemoveAt(index--);
                }
            }

            return granted;
        }

        private bool? FriendshipRuleResult
        {
            get
            {
                if (_friendshipRules.Count == 0)
                {
                    return null;
                }

                bool unresolved = false;
                bool? ruled = null;

                foreach (var abstractSlugcat in TrackedSlugcats)
                {
                    if (source.EvaluateFriendshipRules(abstractSlugcat) is not { } verdict)
                    {
                        unresolved = true;

                        continue;
                    }

                    if (verdict)
                    {
                        return true;
                    }

                    ruled = false;
                }

                return unresolved ? null : ruled;
            }
        }

        private bool? IsCreatureFriend(AbstractCreature abstractSlugcat)
        {
            if (source.EvaluateFriendshipRules(abstractSlugcat) is { } ruled)
            {
                return ruled;
            }

            if (source.IsScavengerArtificerPair(abstractSlugcat))
            {
                return false;
            }

            bool? direct = source.IsDirectCreatureFriend(abstractSlugcat);

            if (direct == true)
            {
                return true;
            }

            foreach (var sharedSlugcat in TrackedSlugcats)
            {
                if (sharedSlugcat == abstractSlugcat || source.IsScavengerArtificerPair(sharedSlugcat))
                {
                    continue;
                }

                bool? sharedRule = source.EvaluateFriendshipRules(sharedSlugcat);

                if ((sharedRule ?? source.IsDirectCreatureFriend(sharedSlugcat)) == true)
                {
                    return true;
                }
            }

            return direct;
        }

        private bool? IsDirectCreatureFriend(AbstractCreature abstractSlugcat)
        {
            if (source.IsSlugcat)
            {
                return true;
            }

            ArtificialIntelligence? aiSource = source?.abstractAI?.RealAI;
            bool hasFriendTracker = aiSource?.friendTracker != null;
            bool hasTracker = aiSource?.tracker != null;

            if (hasFriendTracker && source.IsFriendlyLizard)
            {
                return Config.FriendCreature.IsActive;
            }

            if (hasFriendTracker && source.Likes(abstractSlugcat))
            {
                return Config.FriendCreature.IsActive;
            }

            if (!hasTracker)
            {
                return null;
            }

            CreatureTemplate.Relationship relationship = aiSource!.DynamicRelationship(abstractSlugcat);

            if (relationship.type == CreatureTemplate.Relationship.Type.Pack)
            {
                return Config.FriendCreature.IsActive;
            }

            bool neutral = relationship.type == CreatureTemplate.Relationship.Type.Ignores && source.GetReputation(abstractSlugcat) is not < FriendReputationThreshold;

            return neutral && Config.FriendNeutralCreature.IsActive ? Config.FriendCreature.IsActive : null;
        }

        private bool IsChainedFriend(AbstractCreature target)
        {
            foreach (var sharedSlugcat in TrackedSlugcats)
            {
                if (source.IsFriend(sharedSlugcat, true, false) && target.IsFriend(sharedSlugcat, true, false))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFriend(AbstractCreature? target, bool chaining)
        {
            if (source == null || target == null || source == target || !IsFriendSession)
            {
                return false;
            }

            AbstractCreature? abstractSlugcat = null;
            AbstractCreature? abstractCreature = null;

            if (source.IsSlugcat)
            {
                abstractSlugcat = source;
                abstractCreature = target;
            }
            else if (target.IsSlugcat)
            {
                abstractSlugcat = target;
                abstractCreature = source;
            }

            if (Config.FriendSlugcat.IsActive && abstractSlugcat != null && abstractCreature?.IsCreatureFriend(abstractSlugcat) is { } isCreatureFriend)
            {
                return isCreatureFriend;
            }

            return Config.FriendChaining.IsActive && chaining && !source.IsPlayer && !target.IsPlayer && source.IsChainedFriend(target);
        }
    }

    extension(FriendTracker? tracker)
    {
        internal bool HasSlugcatFriend => tracker?.friend is { } friend && friend.IsSlugcat && friend.abstractCreature is { slatedForDeletion: false, state.dead: false };

        internal void AdoptSlugcatFriend()
        {
            if (
                tracker is not { } friendTracker
                || friendTracker.AI?.creature is not { } abstractCreature
                || abstractCreature.state?.socialMemory is not { } socialMemory
            )
            {
                return;
            }

            foreach (var abstractSlugcat in TrackedSlugcats)
            {
                if (
                    abstractSlugcat == abstractCreature
                    || abstractSlugcat.pos.room != abstractCreature.pos.room
                    || abstractSlugcat.state is not { dead: false }
                    || abstractSlugcat.realizedCreature is not { } slugcat
                )
                {
                    continue;
                }

                if (!TryGetFriendLikes(socialMemory, abstractSlugcat.ID, out float like, out float tempLike))
                {
                    continue;
                }

                SocialMemory.Relationship relationship = _adoptedFriendRelationships.GetValue(friendTracker, _ => new(abstractSlugcat.ID));

                relationship.subjectID = abstractSlugcat.ID;
                relationship.like = like;
                relationship.tempLike = tempLike;

                friendTracker.friend = slugcat;
                friendTracker.friendRel = relationship;

                return;
            }

            if (friendTracker.friend.IsSlugcat)
            {
                friendTracker.friend = null;
                friendTracker.friendRel = null;
            }
        }
    }

    extension(EntityID id)
    {
        public bool IsTrackedSlugcat
        {
            get
            {
                foreach (var abstractSlugcat in TrackedSlugcats)
                {
                    if (abstractSlugcat.ID == id)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    extension(AbstractPhysicalObject? source)
    {
        public bool IsTracked => source is AbstractCreature abstractCreature && TrackedFriendsIncludingPlayers.Contains(abstractCreature);

        public bool IsFriend(AbstractPhysicalObject? target, bool direct = false) => source.IsFriend(target, direct, true);

        public bool IsFriend(PhysicalObject? target, bool direct = false) => source.IsFriend(target?.abstractPhysicalObject, direct);

        public bool IsFriendOfPlayer
        {
            get
            {
                foreach (var abstractPlayer in source?.world?.game?.Players ?? [])
                {
                    if (source.IsFriend(abstractPlayer))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool IsFriend(AbstractPhysicalObject? target, bool direct, bool chaining)
        {
            AbstractCreature? sourceSelf = source as AbstractCreature;
            AbstractCreature? targetSelf = target as AbstractCreature;

            bool friend = sourceSelf.IsFriend(targetSelf, chaining);

            if (!friend && !direct)
            {
                AbstractCreature? sourceOwner = source.Owner;
                AbstractCreature? targetOwner = target.Owner;

                friend = sourceSelf.IsFriend(targetOwner, chaining) || sourceOwner.IsFriend(targetSelf, chaining) || sourceOwner.IsFriend(targetOwner, chaining);
            }

            if (friend)
            {
                Visualizer.FriendLinkOverlay.Track(source?.realizedObject, target?.realizedObject);
            }

            return friend;
        }
    }

    extension(PhysicalObject? source)
    {
        public bool IsTracked => (source?.abstractPhysicalObject).IsTracked;

        public bool IsFriend(AbstractPhysicalObject? target, bool direct = false) => (source?.abstractPhysicalObject).IsFriend(target, direct);

        public bool IsFriend(PhysicalObject? target, bool direct = false) => (source?.abstractPhysicalObject).IsFriend(target?.abstractPhysicalObject, direct);

        public bool IsFriendOfPlayer => (source?.abstractPhysicalObject).IsFriendOfPlayer;
    }

    extension(Room? room)
    {
        public IEnumerable<PhysicalObject> Objects
        {
            get
            {
                foreach (var physicalObjects in room?.physicalObjects ?? [])
                {
                    foreach (var physicalObject in physicalObjects)
                    {
                        if (physicalObject != null)
                        {
                            yield return physicalObject;
                        }
                    }
                }
            }
        }

        public IEnumerable<Creature> Creatures
        {
            get
            {
                foreach (var physicalObject in room.Objects)
                {
                    if (physicalObject is Creature creature)
                    {
                        yield return creature;
                    }
                }
            }
        }

        public IEnumerable<Creature> FriendsOf(PhysicalObject? source)
        {
            foreach (var creature in room.Creatures)
            {
                if (source.IsFriend(creature))
                {
                    yield return creature;
                }
            }
        }

        public IEnumerable<Creature> PlayerFriends
        {
            get
            {
                foreach (var creature in room.Creatures)
                {
                    if (creature.IsFriendOfPlayer)
                    {
                        yield return creature;
                    }
                }
            }
        }
    }

    internal static void ClearTrackedFriends() => _friendSet.Clear();

    internal static void PruneTrackedFriends() => _friendSet.RemoveWhere(abstractCreature => abstractCreature.slatedForDeletion || abstractCreature.world != RainWorldUtils.CurrentGame?.world);
}
