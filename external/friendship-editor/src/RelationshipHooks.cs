using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.FriendshipEditor;

internal class RelationshipHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.RelationshipEditor];

    private static CreatureTemplate.Relationship? Ruled(RelationshipTracker tracker, RelationshipTracker.DynamicRelationship relationship) =>
        tracker.AI?.creature is { } creature && relationship.trackerRep?.representedCreature is { } target ? RuleEffects.RelationshipFor(creature, target) : null;

    [HookPatch(typeof(On.ArtificialIntelligence), nameof(On.ArtificialIntelligence.StaticRelationship))]
    private static CreatureTemplate.Relationship On_ArtificialIntelligence_StaticRelationship(On.ArtificialIntelligence.orig_StaticRelationship orig, ArtificialIntelligence self, AbstractCreature otherCreature)
    {
        CreatureTemplate.Relationship relationship = orig(self, otherCreature);

        return self.creature is { } creature && otherCreature != null && RuleEffects.RelationshipFor(creature, otherCreature) is { } ruled ? ruled : relationship;
    }

    [HookPatch(typeof(On.RelationshipTracker), nameof(On.RelationshipTracker.SortCreatureIntoModule))]
    private static void On_RelationshipTracker_SortCreatureIntoModule(On.RelationshipTracker.orig_SortCreatureIntoModule orig, RelationshipTracker self, RelationshipTracker.DynamicRelationship relCrit, CreatureTemplate.Relationship newRelationship)
    {
        orig(self, relCrit, Ruled(self, relCrit) ?? newRelationship);
    }

    [HookPatch(typeof(On.RelationshipTracker), nameof(On.RelationshipTracker.Update))]
    private static void On_RelationshipTracker_Update(On.RelationshipTracker.orig_Update orig, RelationshipTracker self)
    {
        orig(self);

        for (int index = self.relationships.Count - 1; index >= 0; index--)
        {
            if (index >= self.relationships.Count)
            {
                continue;
            }

            RelationshipTracker.DynamicRelationship relationship = self.relationships[index];

            if (Ruled(self, relationship) is not { } ruled)
            {
                continue;
            }

            if (ruled.type != relationship.currentRelationship.type)
            {
                self.SortCreatureIntoModule(relationship, ruled);
            }

            relationship.trackerRep.priority = ruled.intensity * relationship.trackedByModuleWeigth;
            relationship.currentRelationship = ruled;
        }
    }
}
