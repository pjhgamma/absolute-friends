using AbsoluteFriends.Hooks;
using AbsoluteFriends.Options;
using UnityEngine;

namespace AbsoluteFriends.Core;

internal class FriendRelationshipHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat, Config.FriendCreature, Config.FriendChaining];

    protected override string? Subject => "Relationship";

    private static bool ShouldNeutralize(CreatureTemplate.Relationship.Type type)
    {
        return type != CreatureTemplate.Relationship.Type.DoesntTrack
            && type != CreatureTemplate.Relationship.Type.Ignores
            && type != CreatureTemplate.Relationship.Type.PlaysWith
            && type != CreatureTemplate.Relationship.Type.SocialDependent
            && type != CreatureTemplate.Relationship.Type.Pack;
    }

    [HookPatch(typeof(On.RelationshipTracker.DynamicRelationship), nameof(On.RelationshipTracker.DynamicRelationship.Update))]
    private static void On_DynamicRelationship_Update(On.RelationshipTracker.DynamicRelationship.orig_Update orig, RelationshipTracker.DynamicRelationship self)
    {
        RelationshipTracker relationshipTracker = self.rt;
        Tracker.CreatureRepresentation trackerRep = self.trackerRep;
        AbstractCreature? abstractCreature = relationshipTracker?.AI?.creature;
        AbstractCreature? targetAbstractCreature = trackerRep?.representedCreature;

        if (relationshipTracker == null || trackerRep == null || abstractCreature == null || targetAbstractCreature == null || !abstractCreature.IsFriend(targetAbstractCreature, direct: true))
        {
            orig(self);

            return;
        }

        if (Config.FriendChaining.IsActive && abstractCreature.state?.socialMemory?.GetOrInitiateRelationship(targetAbstractCreature.ID) is { } socialRelationship)
        {
            socialRelationship.like = Mathf.Max(socialRelationship.like, FriendUtils.FriendLikeThreshold);
            socialRelationship.tempLike = Mathf.Max(socialRelationship.tempLike, FriendUtils.FriendLikeThreshold);
            socialRelationship.know = Mathf.Max(socialRelationship.know, FriendUtils.FriendLikeThreshold);
        }

        if (relationshipTracker.AI is not IUseARelationshipTracker relationshipUser)
        {
            orig(self);

            return;
        }

        CreatureTemplate.Relationship relationship = relationshipUser.UpdateDynamicRelationship(self);

        if (ShouldNeutralize(relationship.type))
        {
            relationship = new(CreatureTemplate.Relationship.Type.Ignores, FriendUtils.FriendLikeThreshold);
        }

        if (relationship.type != self.currentRelationship.type)
        {
            relationshipTracker.SortCreatureIntoModule(self, relationship);
        }

        trackerRep.priority = relationship.intensity * self.trackedByModuleWeigth;
        self.currentRelationship = relationship;
    }
}
