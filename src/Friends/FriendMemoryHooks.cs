using RippleFriends.Hooks;
using RippleFriends.Options;
using UnityEngine;

namespace RippleFriends.Friends;

internal class FriendMemoryHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat];

    private static float Share(SocialMemory self, EntityID subjectID, float like, bool temporary)
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

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetLike))]
    private static float On_SocialMemory_GetLike(On.SocialMemory.orig_GetLike orig, SocialMemory self, EntityID subjectID)
    {
        return Share(self, subjectID, orig(self, subjectID), false);
    }

    [HookPatch(typeof(On.SocialMemory), nameof(On.SocialMemory.GetTempLike))]
    private static float On_SocialMemory_GetTempLike(On.SocialMemory.orig_GetTempLike orig, SocialMemory self, EntityID subjectID)
    {
        return Share(self, subjectID, orig(self, subjectID), true);
    }
}
