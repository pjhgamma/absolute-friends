using MoreSlugcats;
using RippleFriends.Diagnostics.Visualizer;
using RippleFriends.Options;
using RippleFriends.Utils;
using RWCustom;
using Watcher;

namespace RippleFriends.Friends;

internal static class FriendUtils
{
    public const float FriendLike = 0.5f;

    public const float FriendReputation = 0f;

    private static readonly HashSet<AbstractCreature> _friendSet = [];

    public static bool IsFriendSession => Config.FriendArena.IsActive || RainWorldUtils.CurrentGame?.IsArenaSession == false;

    private static IEnumerable<AbstractCreature> Players => RainWorldUtils.CurrentGame?.Players ?? [];

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
                if (abstractCreature.IsSlugcat || Config.FriendCreature.IsActive)
                {
                    yield return abstractCreature;
                }
            }
        }
    }

    public static IEnumerable<AbstractCreature> TrackedFriendsWithPlayers
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

    public static void ClearTrackedFriends() => _friendSet.Clear();

    public static void PruneTrackedFriends() => _friendSet.RemoveWhere(abstractCreature => abstractCreature.slatedForDeletion || abstractCreature.world != RainWorldUtils.CurrentGame?.world);

    extension(AbstractCreature? source)
    {
        public void Track()
        {
            if (source is { } abstractCreature)
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

        public bool Like(AbstractCreature? target)
        {
            return source != null
                && target != null
                && source.state?.socialMemory?.GetRelationship(target.ID) is { } relationship
                && relationship.like > FriendLike
                && relationship.tempLike > FriendLike;
        }

        private float? Repute(AbstractCreature? target)
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

        private IEnumerable<AbstractCreature> SharedSlugcats
        {
            get
            {
                if (!source.IsSlugcat || source is not { } abstractSlugcat)
                {
                    yield break;
                }

                yield return abstractSlugcat;

                foreach (var sharedSlugcat in TrackedSlugcats)
                {
                    if (sharedSlugcat != abstractSlugcat)
                    {
                        yield return sharedSlugcat;
                    }
                }
            }
        }

        private bool? IsCreatureFriend(AbstractCreature abstractSlugcat)
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

            bool neutral = false;

            foreach (var sharedSlugcat in abstractSlugcat.SharedSlugcats)
            {
                if (hasFriendTracker && source.Like(sharedSlugcat))
                {
                    return Config.FriendCreature.IsActive;
                }

                if (hasTracker)
                {
                    CreatureTemplate.Relationship relationship = aiSource!.DynamicRelationship(sharedSlugcat);

                    if (relationship.type == CreatureTemplate.Relationship.Type.Pack)
                    {
                        return Config.FriendCreature.IsActive;
                    }

                    neutral |= relationship.type == CreatureTemplate.Relationship.Type.Ignores && source.Repute(sharedSlugcat) is not < FriendReputation;
                }
            }

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

            if (Config.FriendIterator.IsActive && abstractSlugcat != null && abstractCreature is AbstractOwner abstractOwner)
            {
                return abstractOwner.IsIteratorFriend(abstractSlugcat);
            }

            return Config.FriendChaining.IsActive && chaining && !source.IsPlayer && !target.IsPlayer && source.IsChainedFriend(target);
        }
    }

    extension(AbstractOwner abstractOwner)
    {
        private bool IsIteratorFriend(AbstractCreature abstractSlugcat)
        {
            return abstractOwner.PhysicalObject switch
            {
                Oracle => RainWorldUtils.CurrentGame is not { IsStorySession: true } game
                    || game.TimelinePoint != SlugcatStats.Timeline.Saint
                    || abstractSlugcat.realizedObject is not Player { monkAscension: true },
                SLOracleSwarmer => true,
                HalcyonPearl => true,
                Prince => true,
                _ => false
            };
        }
    }

    extension(FriendTracker? tracker)
    {
        public bool HasSlugcatFriend => tracker?.friend is { } friend && friend.IsSlugcat && friend.abstractCreature is { slatedForDeletion: false, state.dead: false };

        public void AdoptSlugcatFriend()
        {
            if (
                tracker is not { } friendTracker
                || friendTracker.HasSlugcatFriend
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

                float like = socialMemory.GetLike(abstractSlugcat.ID);

                if (like <= FriendLike)
                {
                    continue;
                }

                float tempLike = socialMemory.GetTempLike(abstractSlugcat.ID);

                if (tempLike == 0f)
                {
                    tempLike = like;
                }

                if (tempLike <= FriendLike)
                {
                    continue;
                }

                SocialMemory.Relationship relationship = socialMemory.GetOrInitiateRelationship(abstractSlugcat.ID);

                relationship.like = like;
                relationship.tempLike = tempLike;

                friendTracker.friend = slugcat;
                friendTracker.friendRel = relationship;

                return;
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
        public bool IsTracked => source is AbstractCreature abstractCreature && TrackedFriendsWithPlayers.Contains(abstractCreature);

        public bool IsFriend(AbstractPhysicalObject? target, bool direct = false) => source.IsFriend(target, direct, true);

        public bool IsFriend(PhysicalObject? target, bool direct = false) => source.IsFriend(target?.abstractPhysicalObject, direct);

        public bool IsPlayerFriend
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
                FriendLinkOverlay.Track(source?.realizedObject, target?.realizedObject);
            }

            return friend;
        }
    }

    extension(PhysicalObject? source)
    {
        public bool IsTracked => (source?.abstractPhysicalObject).IsTracked;

        public bool IsFriend(AbstractPhysicalObject? target, bool direct = false) => (source?.abstractPhysicalObject).IsFriend(target, direct);

        public bool IsFriend(PhysicalObject? target, bool direct = false) => (source?.abstractPhysicalObject).IsFriend(target?.abstractPhysicalObject, direct);

        public bool IsPlayerFriend => (source?.abstractPhysicalObject).IsPlayerFriend;
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

        public IEnumerable<Creature> FriendsOfPlayer
        {
            get
            {
                foreach (var creature in room.Creatures)
                {
                    if (creature.IsPlayerFriend)
                    {
                        yield return creature;
                    }
                }
            }
        }

        public IEnumerable<AbstractPhysicalObject> TaggedObjects
        {
            get
            {
                foreach (var abstractCreature in TrackedFriends)
                {
                    yield return abstractCreature;
                }

                foreach (var physicalObject in room.Objects)
                {
                    if (!physicalObject.IsTracked && (physicalObject.Owner != null || (physicalObject is Creature && physicalObject.IsPlayerFriend)))
                    {
                        yield return physicalObject.abstractPhysicalObject;
                    }
                }
            }
        }
    }
}
