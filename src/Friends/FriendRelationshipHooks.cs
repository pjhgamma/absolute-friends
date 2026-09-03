using RippleFriends.Hooks;
using RippleFriends.Options;
using UnityEngine;

namespace RippleFriends.Friends;

internal class FriendRelationshipHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat];

    protected override string? Subject => "Relationship";

    private static bool IsAggressive(CreatureTemplate.Relationship.Type type)
    {
        return type == CreatureTemplate.Relationship.Type.Attacks
            || type == CreatureTemplate.Relationship.Type.Eats
            || type == CreatureTemplate.Relationship.Type.AgressiveRival
            || type == CreatureTemplate.Relationship.Type.Antagonizes;
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
            socialRelationship.like = Mathf.Max(socialRelationship.like, FriendUtils.FriendLike);
            socialRelationship.tempLike = Mathf.Max(socialRelationship.tempLike, FriendUtils.FriendLike);
            socialRelationship.know = Mathf.Max(socialRelationship.know, FriendUtils.FriendLike);
        }

        if (relationshipTracker.AI is not IUseARelationshipTracker relationshipUser)
        {
            orig(self);

            return;
        }

        CreatureTemplate.Relationship relationship = relationshipUser.UpdateDynamicRelationship(self);

        if (IsAggressive(relationship.type))
        {
            relationship = new(CreatureTemplate.Relationship.Type.Ignores, FriendUtils.FriendLike);
        }

        if (relationship.type != self.currentRelationship.type)
        {
            relationshipTracker.SortCreatureIntoModule(self, relationship);
        }

        trackerRep.priority = relationship.intensity * self.trackedByModuleWeigth;
        self.currentRelationship = relationship;
    }
}
