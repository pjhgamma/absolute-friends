using System.Runtime.CompilerServices;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

public static partial class FriendUtils
{
    private static readonly ConditionalWeakTable<FriendTracker, SocialMemory.Relationship> _adoptedRelationships = new();

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
}
