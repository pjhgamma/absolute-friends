using System.Runtime.CompilerServices;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

public static partial class FriendUtils
{
    private static ConditionalWeakTable<AbstractCreature, HashSet<EntityID>> _trackedPlayerIds = new();

    private static ConditionalWeakTable<AbstractCreature, HashSet<EntityID>> _pupFriendIds = new();

    private static readonly HashSet<AbstractCreature> _trackedSlugcats = [];

    private static readonly HashSet<AbstractCreature> _trackedCreatures = [];

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
                if (abstractCreature.IsTrackingAllowed)
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

            foreach (var abstractSlugpup in _trackedSlugcats)
            {
                yield return abstractSlugpup;
            }
        }
    }

    internal static bool HasMultipleTrackedSlugcats => (RainWorldUtils.CurrentGame?.Players.Count ?? 0) + _trackedSlugcats.Count > 1;

    private static IEnumerable<AbstractCreature> Players => RainWorldUtils.CurrentGame?.Players ?? [];

    internal static void ClearTrackedFriends()
    {
        _trackedSlugcats.Clear();
        _trackedCreatures.Clear();
        _chainingFriendships.Clear();
        _chainingGame = null;
        _chainingClock = -1;
        _trackedPlayerIds = new();
        _pupFriendIds = new();
    }

    internal static void PruneTrackedFriends()
    {
        _trackedCreatures.RemoveWhere(abstractCreature =>
            (abstractCreature.slatedForDeletion && abstractCreature.state?.dead != true)
            || abstractCreature.world != RainWorldUtils.CurrentGame?.world
        );
        _trackedSlugcats.RemoveWhere(abstractCreature => !_trackedCreatures.Contains(abstractCreature));
    }

    extension(AbstractCreature? source)
    {
        public void Track()
        {
            if (source is { } abstractCreature and not AbstractOwner)
            {
                if (abstractCreature.IsNPC)
                {
                    _trackedSlugcats.Add(abstractCreature);
                }

                _trackedCreatures.Add(abstractCreature);

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
        }

        public void Untrack()
        {
            if (source is { } abstractCreature)
            {
                _trackedSlugcats.Remove(abstractCreature);
                _trackedCreatures.Remove(abstractCreature);
                _trackedPlayerIds.Remove(abstractCreature);
                _pupFriendIds.Remove(abstractCreature);
            }
        }

        public bool IsTrackedFor(AbstractCreature? abstractPlayer)
        {
            if (source == null || abstractPlayer == null || !_trackedCreatures.Contains(source))
            {
                return false;
            }

            if (source.IsFriend(abstractPlayer))
            {
                return true;
            }

            return source.abstractAI?.RealAI == null
                && source.GetFriendshipVerdict(abstractPlayer) != false
                && _trackedPlayerIds.TryGetValue(source, out HashSet<EntityID> playerIds)
                && playerIds.Contains(abstractPlayer.ID);
        }

        internal void TrackPupFriend(AbstractCreature abstractPlayer)
        {
            if (source is not { } abstractSlugpup || !abstractSlugpup.IsNPC || !abstractPlayer.IsPlayer)
            {
                return;
            }

            _pupFriendIds.GetOrCreateValue(abstractSlugpup).Add(abstractPlayer.ID);
            abstractSlugpup.Track();
        }

        internal bool HasRemotePupFriend
        {
            get
            {
                if (source == null || !_trackedCreatures.Contains(source) || !_pupFriendIds.TryGetValue(source, out HashSet<EntityID> playerIds))
                {
                    return false;
                }

                foreach (var abstractPlayer in Players)
                {
                    if (
                        playerIds.Contains(abstractPlayer.ID)
                        && source.state?.socialMemory?.GetRelationship(abstractPlayer.ID) is { like: > FriendLikeThreshold, tempLike: > FriendLikeThreshold }
                        && (abstractPlayer.realizedCreature == null || source.pos.room != abstractPlayer.pos.room)
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool IsTrackedFriend
        {
            get
            {
                if (!IsFriendSession || source == null)
                {
                    return false;
                }

                if (Config.FriendSlugcat.IsActive && RainWorldUtils.CurrentGame?.Players.Contains(source) == true)
                {
                    return true;
                }

                return _trackedCreatures.Contains(source) && source.IsTrackingAllowed;
            }
        }

        private bool IsTrackingAllowed
        {
            get
            {
                if (source == null)
                {
                    return false;
                }

                if (source.IsSlugcat)
                {
                    return Config.FriendSlugcat.IsActive && source.GetTrackedFriendshipVerdict() != false;
                }

                return !source.IsDenied
                    && (source.GetTrackedFriendshipVerdict() ?? source.IsFriendlyAllowed);
            }
        }
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

            if (!Config.FriendSharing.IsActive || abstractCreature.IsDenied || abstractCreature.state?.socialMemory is not { } socialMemory)
            {
                if (adopted)
                {
                    tracker.friend = null;
                    tracker.friendRel = null;
                }

                return;
            }

            if (!adopted && (tracker.friend != null || tracker.friendRel != null))
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
                    || !TryGetFriendlyLikes(socialMemory, abstractSlugcat.ID, out float like, out float tempLike)
                )
                {
                    continue;
                }

                SocialMemory.Relationship relationship = _adoptedRelationships.GetValue(tracker, _ => new(abstractSlugcat.ID));

                relationship.subjectID = abstractSlugcat.ID;
                relationship.like = like;
                relationship.tempLike = tempLike;

                tracker.friend = slugcat;
                tracker.friendRel = relationship;

                return;
            }

            if (adopted)
            {
                tracker.friend = null;
                tracker.friendRel = null;
            }
        }
    }

    extension(EntityID id)
    {
        public bool IsTrackedSlugcat
        {
            get
            {
                if (!IsFriendSession)
                {
                    return false;
                }

                foreach (var abstractSlugcat in Players)
                {
                    if (abstractSlugcat.ID == id)
                    {
                        return true;
                    }
                }

                foreach (var abstractSlugcat in _trackedSlugcats)
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
}
