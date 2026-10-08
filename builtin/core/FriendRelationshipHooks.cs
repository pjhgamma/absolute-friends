using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Core;

internal class FriendRelationshipHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FriendSlugcat, Config.FriendCreature, Config.FriendNeutralCreature, Config.FriendChaining];

    protected override bool IsOptionEnabled => base.IsOptionEnabled || CreatureRules.HasConfiguredRules;

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
        if (self is not { rt: { AI.creature: { } source } relationshipTracker, trackerRep: { representedCreature: { } target } trackerRep }
            || !source.IsFriend(target, direct: true))
        {
            orig(self);
            self.RecordVanillaRelationship(self.currentRelationship);

            return;
        }

        if (relationshipTracker.AI is not IUseARelationshipTracker relationshipUser)
        {
            orig(self);
            self.RecordVanillaRelationship(self.currentRelationship);

            return;
        }

        CreatureTemplate.Relationship relationship = relationshipUser.UpdateDynamicRelationship(self);

        self.RecordVanillaRelationship(relationship);

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
