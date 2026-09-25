using AbsoluteFriends.Hooks;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AbsoluteFriends.Core;

internal class FriendMemoryHooks : BaseHooks
{
    private static readonly ConditionalWeakTable<SocialMemory.Relationship, SocialMemory> _relationshipMemories = new();

    protected override Configurable<bool>[] Options => [Config.FriendSharing];

    private static IEnumerable<int> SharedPlayerNumbers(CreatureCommunities communities, int playerNumber)
    {
        if (!FriendUtils.IsFriendSession || communities.session is StoryGameSession)
        {
            yield break;
        }

        int[] playerNumbers = [.. FriendUtils.TrackedSlugcats
            .Select(abstractSlugcat => abstractSlugcat.state)
            .OfType<PlayerState>()
            .Select(playerState => playerState.playerNumber)
            .Distinct()
        ];

        if (!playerNumbers.Contains(playerNumber))
        {
            yield break;
        }

        foreach (int sharedPlayerNumber in playerNumbers)
        {
            if (sharedPlayerNumber != playerNumber)
            {
                yield return sharedPlayerNumber;
            }
        }
    }

    private static IEnumerable<SocialMemory.Relationship> SharedRelationships(SocialMemory.Relationship relationship)
    {
        if (
            !FriendUtils.IsFriendSession
            || !relationship.subjectID.IsTrackedSlugcat
            || !_relationshipMemories.TryGetValue(relationship, out SocialMemory socialMemory)
        )
        {
            yield break;
        }

        foreach (var abstractSlugcat in FriendUtils.TrackedSlugcats)
        {
            if (abstractSlugcat.ID != relationship.subjectID)
            {
                yield return socialMemory.GetOrInitiateRelationship(abstractSlugcat.ID);
            }
        }
    }

    private static float ShareLike(SocialMemory self, EntityID subjectID, float like, bool temporary)
    {
        if (!FriendUtils.IsFriendSession || !subjectID.IsTrackedSlugcat)
        {
            return like;
        }

        foreach (var abstractSlugcat in FriendUtils.TrackedSlugcats)
        {
            if (abstractSlugcat.ID != subjectID && self.GetRelationship(abstractSlugcat.ID) is { } relationship)
            {
                like = Mathf.Max(like, temporary ? relationship.tempLike : relationship.like);
            }
        }

        return like;
    }

    private static float ShareKnow(SocialMemory self, EntityID subjectID, float know)
    {
        if (!FriendUtils.IsFriendSession || !subjectID.IsTrackedSlugcat)
        {
            return know;
        }

        foreach (var abstractSlugcat in FriendUtils.TrackedSlugcats)
        {
            if (abstractSlugcat.ID != subjectID && self.GetRelationship(abstractSlugcat.ID) is { } relationship)
            {
                know = Mathf.Max(know, relationship.know);
            }
        }

        return know;
    }

    private static SocialMemory.Relationship? TrackRelationshipMemory(SocialMemory self, SocialMemory.Relationship? relationship)
    {
        if (relationship != null)
        {
            _relationshipMemories.GetValue(relationship, _ => self);
        }

        return relationship;
    }

    [HookPatch(typeof(On.CreatureCommunities), nameof(On.CreatureCommunities.InfluenceLikeOfPlayer))]
    private static void On_CreatureCommunities_InfluenceLikeOfPlayer(On.CreatureCommunities.orig_InfluenceLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID communityID, int region, int playerNumber, float influence, float interRegionBleed, float interCommunityBleed)
    {
        orig(self, communityID, region, playerNumber, influence, interRegionBleed, interCommunityBleed);

        foreach (int sharedPlayerNumber in SharedPlayerNumbers(self, playerNumber))
        {
            orig(self, communityID, region, sharedPlayerNumber, influence, interRegionBleed, interCommunityBleed);
        }
    }

    [HookPatch(typeof(On.CreatureCommunities), nameof(On.CreatureCommunities.LikeOfPlayer))]
    private static float On_CreatureCommunities_LikeOfPlayer(On.CreatureCommunities.orig_LikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID communityID, int region, int playerNumber)
    {
        float reputation = orig(self, communityID, region, playerNumber);

        foreach (int sharedPlayerNumber in SharedPlayerNumbers(self, playerNumber))
        {
            reputation = Mathf.Max(reputation, orig(self, communityID, region, sharedPlayerNumber));
        }

        return reputation;
    }

    [HookPatch(typeof(On.CreatureCommunities), nameof(On.CreatureCommunities.SetLikeOfPlayer))]
    private static void On_CreatureCommunities_SetLikeOfPlayer(On.CreatureCommunities.orig_SetLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID communityID, int region, int playerNumber, float like)
    {
        orig(self, communityID, region, playerNumber, like);

        foreach (int sharedPlayerNumber in SharedPlayerNumbers(self, playerNumber))
        {
            orig(self, communityID, region, sharedPlayerNumber, like);
        }
    }

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetKnow))]
    private static float On_SocialMemory_GetKnow(On.SocialMemory.orig_GetKnow orig, SocialMemory self, EntityID subjectID)
    {
        return ShareKnow(self, subjectID, orig(self, subjectID));
    }

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetLike))]
    private static float On_SocialMemory_GetLike(On.SocialMemory.orig_GetLike orig, SocialMemory self, EntityID subjectID)
    {
        return ShareLike(self, subjectID, orig(self, subjectID), false);
    }

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetOrInitiateRelationship))]
    private static SocialMemory.Relationship On_SocialMemory_GetOrInitiateRelationship(On.SocialMemory.orig_GetOrInitiateRelationship orig, SocialMemory self, EntityID subjectID)
    {
        return TrackRelationshipMemory(self, orig(self, subjectID))!;
    }

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetRelationship))]
    private static SocialMemory.Relationship? On_SocialMemory_GetRelationship(On.SocialMemory.orig_GetRelationship orig, SocialMemory self, EntityID subjectID)
    {
        return TrackRelationshipMemory(self, orig(self, subjectID));
    }

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetTempLike))]
    private static float On_SocialMemory_GetTempLike(On.SocialMemory.orig_GetTempLike orig, SocialMemory self, EntityID subjectID)
    {
        return ShareLike(self, subjectID, orig(self, subjectID), true);
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceKnow))]
    private static void On_Relationship_InfluenceKnow(On.SocialMemory.Relationship.orig_InfluenceKnow orig, SocialMemory.Relationship self, float amountOfLerpUpwards)
    {
        orig(self, amountOfLerpUpwards);

        foreach (var relationship in SharedRelationships(self))
        {
            orig(relationship, amountOfLerpUpwards);
        }
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceLike))]
    private static void On_Relationship_InfluenceLike(On.SocialMemory.Relationship.orig_InfluenceLike orig, SocialMemory.Relationship self, float change)
    {
        orig(self, change);

        foreach (var relationship in SharedRelationships(self))
        {
            orig(relationship, change);
        }
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceTempLike))]
    private static void On_Relationship_InfluenceTempLike(On.SocialMemory.Relationship.orig_InfluenceTempLike orig, SocialMemory.Relationship self, float change)
    {
        orig(self, change);

        foreach (var relationship in SharedRelationships(self))
        {
            orig(relationship, change);
        }
    }
}
