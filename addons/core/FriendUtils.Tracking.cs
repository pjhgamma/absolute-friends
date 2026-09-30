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

    private static readonly HashSet<AbstractCreature> _trackerCreatures = [];

    private static ConditionalWeakTable<RainWorldGame, TrackingChoices> _trackingChoices = new();

    private static readonly ConditionalWeakTable<FriendTracker, SocialMemory.Relationship> _adoptedRelationships = new();

    private static readonly ConditionalWeakTable<FriendTracker, AdoptionOrigin> _adoptionOrigins = new();

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

    internal static IEnumerable<AbstractCreature> ManageableFriends => Players
        .Concat(_trackerCreatures)
        .Distinct()
        .Where(creature => (!creature.slatedForDeletion || creature.state?.dead == true) && creature.IsTrackingAllowed);

    private static IEnumerable<AbstractCreature> Players => RainWorldUtils.CurrentGame?.Players ?? [];

    internal static void ClearTrackedFriends()
    {
        _trackedSlugpups.Clear();
        _trackedCreatures.Clear();
        _trackerCreatures.Clear();
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
        _trackerCreatures.RemoveWhere(IsStale);
    }

    internal static void ResetTrackingChoices() => _trackingChoices = new();

    private static TrackingChoices? ChoicesFor(AbstractCreature creature) => creature.Game is { } game
        ? _trackingChoices.GetValue(game, game => new(game.StorySaveState))
        : null;

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
                    && ChoicesFor(source)?.Exclusions.Contains(source.SaveDataKey) != true;
            }
        }

        internal bool IsSlugpupWithAbsentFriend
        {
            get
            {
                if (source == null || !_trackedCreatures.Contains(source) || !_slugpupFriendIds.TryGetValue(source, out HashSet<EntityID> playerIds))
                {
                    return false;
                }

                foreach (var abstractPlayer in Players)
                {
                    if (
                        playerIds.Contains(abstractPlayer.ID)
                        && abstractPlayer is { slatedForDeletion: false, state.dead: false }
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

        public void Track() => source.Track(fromTracker: false);

        public void Untrack()
        {
            if (source is { } abstractCreature)
            {
                _trackedSlugpups.Remove(abstractCreature);
                _trackedCreatures.Remove(abstractCreature);
                _trackerCreatures.Remove(abstractCreature);
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

        internal void Track(bool fromTracker)
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

            if (fromTracker)
            {
                _trackerCreatures.Add(abstractCreature);
            }

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
            abstractSlugpup.Track(fromTracker: true);
        }

        internal void SetTracked(bool tracked)
        {
            if (source is not { IsTrackingAllowed: true } creature || ChoicesFor(creature) is not { } choices)
            {
                return;
            }

            string key = creature.SaveDataKey;

            if (!tracked && !creature.IsPlayer && !_trackerCreatures.Contains(creature) && !choices.Exclusions.Contains(key))
            {
                return;
            }

            bool changed = tracked ? choices.Exclusions.Remove(key) : choices.Exclusions.Add(key);

            if (changed)
            {
                choices.Write();
            }
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

                _adoptionOrigins.Remove(tracker);

                return;
            }

            if (_adoptionOrigins.TryGetValue(tracker, out AdoptionOrigin origin)
                && origin.Creature is { slatedForDeletion: false, state.dead: false, realizedCreature: { } originalFriend }
                && !originalFriend.dead
                && abstractCreature.IsFriend(origin.Creature))
            {
                tracker.friend = originalFriend;
                tracker.friendRel = origin.Relationship;
                _adoptionOrigins.Remove(tracker);

                return;
            }

            if (abstractCreature.IsSlugpup
                && tracker.friend?.abstractCreature?.IsPlayer != true
                && (!_slugpupFriendIds.TryGetValue(abstractCreature, out HashSet<EntityID> playerIds)
                    || !playerIds.Any(id => abstractCreature.state?.socialMemory?.GetRelationship(id) is { like: > FriendLikeThreshold, tempLike: > FriendLikeThreshold })))
            {
                return;
            }

            if (!adopted && tracker.friend is { IsSlugcat: false })
            {
                return;
            }

            if (tracker.friend is { } currentFriend
                && currentFriend.abstractCreature is { slatedForDeletion: false, state.dead: false } currentAbstractFriend
                && (!adopted || abstractCreature.IsFriend(currentAbstractFriend)))
            {
                return;
            }

            if (!adopted && tracker.friend?.abstractCreature is { IsSlugcat: true } originalCreature)
            {
                _adoptionOrigins.GetValue(tracker, _ => new(originalCreature, tracker.friendRel));
            }

            tracker.friend = null;
            tracker.friendRel = null;

            foreach (var abstractSlugcat in KnownSlugcats)
            {
                if (
                    abstractSlugcat == abstractCreature
                    || abstractSlugcat is not { slatedForDeletion: false, state.dead: false, realizedCreature: { } slugcat }
                    || abstractCreature.IsSlugpup
                    || !abstractCreature.IsFriend(abstractSlugcat)
                )
                {
                    continue;
                }

                float like = 1f;
                float tempLike = 1f;

                if (abstractCreature.state?.socialMemory is { } socialMemory
                    && TryGetLikes(socialMemory, abstractSlugcat.ID, out float rememberedLike, out float rememberedTempLike))
                {
                    like = rememberedLike;
                    tempLike = rememberedTempLike;
                }

                SocialMemory.Relationship relationship = _adoptedRelationships.GetValue(tracker, _ => new(abstractSlugcat.ID));

                relationship.subjectID = abstractSlugcat.ID;
                relationship.like = like;
                relationship.tempLike = tempLike;

                tracker.friend = slugcat;
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

    private sealed class TrackingChoices
    {
        private const string SaveKey = "ABSOLUTEFRIENDS_TRACKING<svB>";

        private const string ExcludedSuffix = "=0";

        internal readonly HashSet<string> Exclusions = new(StringComparer.Ordinal);

        private readonly SaveState? _save;

        internal TrackingChoices(SaveState? save)
        {
            _save = save;

            foreach (string entry in FriendSaveData.ReadEntries(save, SaveKey))
            {
                if (entry.EndsWith(ExcludedSuffix, StringComparison.Ordinal))
                {
                    Exclusions.Add(entry.Substring(0, entry.Length - ExcludedSuffix.Length));
                }
            }
        }

        internal void Write() => FriendSaveData.WriteEntries(_save, SaveKey, Exclusions
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => key + ExcludedSuffix));
    }

    private sealed class AdoptionOrigin(AbstractCreature creature, SocialMemory.Relationship? relationship)
    {
        public AbstractCreature Creature { get; } = creature;

        public SocialMemory.Relationship? Relationship { get; } = relationship;
    }
}
