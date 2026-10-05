using System.Runtime.CompilerServices;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

public static partial class FriendUtils
{
    private static ConditionalWeakTable<AbstractCreature, HashSet<EntityID>> _trackedPlayerIds = new();

    private static ConditionalWeakTable<AbstractCreature, HashSet<EntityID>> _slugpupFriendIds = new();

    private static readonly HashSet<AbstractCreature> _trackedSlugpups = [];

    private static readonly HashSet<AbstractCreature> _trackedCreatures = [];

    private static readonly List<Func<AbstractCreature, bool>> _trackingRules = [];

    private static readonly ConditionalWeakTable<FriendTracker, SocialMemory.Relationship> _adoptedRelationships = new();

    public static IEnumerable<AbstractCreature> TrackedFriends
    {
        get
        {
            if (!IsFriendSession)
            {
                yield break;
            }

            foreach (var abstractCreature in _trackedCreatures)
            {
                if (abstractCreature.IsTracked)
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
            if (IsFriendSession && Config.FriendSlugcat.IsActive)
            {
                foreach (var abstractPlayer in Players)
                {
                    if (abstractPlayer.IsTracked)
                    {
                        yield return abstractPlayer;
                    }
                }
            }

            foreach (var abstractCreature in TrackedFriends)
            {
                yield return abstractCreature;
            }
        }
    }

    public static IEnumerable<AbstractCreature> KnownSlugcats
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

            foreach (var abstractSlugpup in _trackedSlugpups)
            {
                yield return abstractSlugpup;
            }
        }
    }

    internal static bool HasMultipleKnownSlugcats => (RainWorldUtils.CurrentGame?.Players.Count ?? 0) + _trackedSlugpups.Count > 1;

    public static IEnumerable<AbstractCreature> TrackingCandidates => Players
        .Concat(_trackedCreatures)
        .Distinct()
        .Where(creature => (!creature.slatedForDeletion || creature.state?.dead == true) && creature.IsTrackingAllowed);

    private static IEnumerable<AbstractCreature> Players => RainWorldUtils.CurrentGame?.Players ?? [];

    internal static void ClearTrackedFriends()
    {
        _trackedSlugpups.Clear();
        _trackedCreatures.Clear();
        _chainingFriendships.Clear();
        _chainingGame = null;
        _chainingClock = -1;
        _trackedPlayerIds = new();
        _slugpupFriendIds = new();
    }

    internal static void PruneTrackedFriends()
    {
        RainWorldGame? game = RainWorldUtils.CurrentGame;

        bool IsStale(AbstractCreature abstractCreature) =>
            (abstractCreature.slatedForDeletion && abstractCreature.state?.dead != true)
            || abstractCreature.world != game?.world;

        _trackedCreatures.RemoveWhere(IsStale);
        _trackedSlugpups.RemoveWhere(abstractCreature => !_trackedCreatures.Contains(abstractCreature));
    }

    public static void RegisterTrackingRule(Func<AbstractCreature, bool> rule)
    {
        if (rule != null && !_trackingRules.Contains(rule))
        {
            _trackingRules.Add(rule);
        }
    }

    public static void UnregisterTrackingRule(Func<AbstractCreature, bool> rule) => _trackingRules.Remove(rule);

    extension(AbstractCreature? source)
    {
        public bool IsTracked
        {
            get
            {
                if (source == null || (!_trackedCreatures.Contains(source) && RainWorldUtils.CurrentGame?.Players.Contains(source) != true))
                {
                    return false;
                }

                return source.IsTrackingAllowed
                    && (source.IsPlayer || _trackingRules.All(rule => rule(source)));
            }
        }

        internal bool IsCloseFriendOfPlayer => IsFriendSession
            && source is { } abstractCreature
            && !abstractCreature.IsDenied
            && Players.Any(abstractPlayer => abstractCreature.IsCloseFriendOf(abstractPlayer));

        internal AbstractCreature? FavoredPlayer => Players.FirstOrDefault(abstractPlayer =>
            abstractPlayer is { slatedForDeletion: false, state.dead: false } && source.CloseRelationshipWith(abstractPlayer.ID) != null);

        private bool IsCloseFriendOf(AbstractCreature abstractSlugcat) => !abstractSlugcat.IsDenied
            && source.GetCreatureFriendship(abstractSlugcat, allowNeutral: false) == true;

        private bool IsTrackingAllowed
        {
            get
            {
                if (!IsFriendSession || source == null)
                {
                    return false;
                }

                if (source.IsSlugcat)
                {
                    return Config.FriendSlugcat.IsActive && source.GetSlugcatFriendshipVerdict() != false;
                }

                return !source.IsDenied
                    && (source.GetSlugcatFriendshipVerdict() ?? source.IsFriendlyAllowed);
            }
        }

        public void Untrack()
        {
            if (source is { } abstractCreature)
            {
                _trackedSlugpups.Remove(abstractCreature);
                _trackedCreatures.Remove(abstractCreature);
                _trackedPlayerIds.Remove(abstractCreature);
            }
        }

        public bool IsTrackedFor(AbstractCreature? abstractPlayer)
        {
            if (source is not { IsTracked: true } || abstractPlayer == null)
            {
                return false;
            }

            if (source.IsFriend(abstractPlayer))
            {
                return true;
            }

            return source.abstractAI?.RealAI == null
                && source.RememberedFriendshipWith(abstractPlayer) == true;
        }

        public void Track()
        {
            if (source is not { } abstractCreature || abstractCreature is AbstractOwner)
            {
                return;
            }

            if (abstractCreature.IsSlugpup)
            {
                _trackedSlugpups.Add(abstractCreature);
            }

            _trackedCreatures.Add(abstractCreature);

            if (!abstractCreature.IsSlugcat && abstractCreature.abstractAI?.RealAI == null)
            {
                return;
            }

            HashSet<EntityID> playerIds = _trackedPlayerIds.GetOrCreateValue(abstractCreature);

            playerIds.Clear();

            foreach (var abstractPlayer in Players)
            {
                if (abstractCreature.IsFriend(abstractPlayer))
                {
                    playerIds.Add(abstractPlayer.ID);
                }
            }
        }

        internal void TrackSlugpupFriend(AbstractCreature abstractPlayer)
        {
            if (source is not { } abstractSlugpup || !abstractSlugpup.IsSlugpup || !abstractPlayer.IsPlayer)
            {
                return;
            }

            _slugpupFriendIds.GetOrCreateValue(abstractSlugpup).Add(abstractPlayer.ID);
            abstractSlugpup.Track();
        }

        private bool? RememberedFriendshipWith(AbstractCreature target)
        {
            if (source == null)
            {
                return null;
            }

            if (source.IsDenied || target.IsDenied || source.GetFriendshipVerdict(target) == false)
            {
                return false;
            }

            return _trackedPlayerIds.TryGetValue(source, out HashSet<EntityID> playerIds)
                ? playerIds.Contains(target.ID)
                : null;
        }

        private SocialMemory.Relationship? CloseRelationshipWith(EntityID id) =>
            source?.state?.socialMemory?.GetRelationship(id) is { like: > FriendLikeThreshold, tempLike: > FriendLikeThreshold } relationship ? relationship : null;
    }

    extension(FriendTracker? tracker)
    {
        internal bool HasSlugcatFriend => tracker?.friend is { IsSlugcat: true, abstractCreature: { slatedForDeletion: false, state.dead: false } };

        internal void AdoptSlugcatFriend()
        {
            if (tracker?.AI?.creature is not { } abstractCreature)
            {
                return;
            }

            bool adopted = _adoptedRelationships.TryGetValue(tracker, out SocialMemory.Relationship adoptedRelationship)
                && ReferenceEquals(tracker.friendRel, adoptedRelationship);

            if (!Config.FriendSharing.IsActive || abstractCreature.IsDenied)
            {
                if (adopted)
                {
                    tracker.friend = null;
                    tracker.friendRel = null;
                }

                return;
            }

            if (!adopted && tracker.friend is { IsSlugcat: false })
            {
                return;
            }

            if (
                tracker.friend?.abstractCreature is { IsPlayer: true, slatedForDeletion: false, state.dead: false } currentFriend
                && abstractCreature.IsFriend(currentFriend)
            )
            {
                return;
            }

            if (adopted)
            {
                tracker.friend = null;
                tracker.friendRel = null;
            }

            if (abstractCreature.state?.socialMemory is not { } socialMemory)
            {
                return;
            }

            foreach (var abstractPlayer in Players)
            {
                if (
                    abstractPlayer is not { slatedForDeletion: false, state.dead: false, realizedCreature: { } player }
                    || abstractPlayer.Room != abstractCreature.Room
                    || !abstractCreature.IsFriend(abstractPlayer)
                    || !TryGetLikes(socialMemory, abstractPlayer.ID, out float like, out float tempLike)
                )
                {
                    continue;
                }

                SocialMemory.Relationship relationship = _adoptedRelationships.GetValue(tracker, _ => new(abstractPlayer.ID));

                relationship.subjectID = abstractPlayer.ID;
                relationship.like = like;
                relationship.tempLike = tempLike;

                tracker.friend = player;
                tracker.friendRel = relationship;

                return;
            }
        }
    }

    extension(EntityID id)
    {
        public bool IsKnownSlugcat
        {
            get
            {
                if (!IsFriendSession)
                {
                    return false;
                }

                foreach (var abstractPlayer in Players)
                {
                    if (abstractPlayer.ID == id)
                    {
                        return true;
                    }
                }

                foreach (var abstractSlugpup in _trackedSlugpups)
                {
                    if (abstractSlugpup.ID == id)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
