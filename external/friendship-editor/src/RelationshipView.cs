using AbsoluteFriends.Manager;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class RelationshipView(FriendTab tab, RainWorldGame game, float y) : CreatureView(tab, game, y, Stat.Relationship)
{
    internal override string Title => "Relationship";

    internal static bool IsAvailableFor(AbstractCreature friend) => Config.RelationshipEditor.IsActive && IsFriendTarget(friend);

    private static CreatureTemplate.Relationship? Tracked(AbstractCreature creature, AbstractCreature target) =>
        creature.abstractAI?.RealAI?.tracker?.RepresentationForCreature(target, false)?.dynamicRelationship?.currentRelationship;

    internal override bool IsVisibleFor(AbstractCreature friend) => IsAvailableFor(friend);

    protected override bool Knows(AbstractCreature creature, AbstractCreature target) => creature.abstractAI?.RealAI?.tracker?.RepresentationForCreature(target, false) != null;

    protected override void ShowRow(CreatureRow row, AbstractCreature creature, AbstractCreature target, float y)
    {
        CreatureTemplate.Relationship? tracked = Tracked(creature, target);

        row.ShowRelationship(creature, tracked ?? RuleEffects.RelationshipFor(creature, target) ?? creature.creatureTemplate.CreatureRelationship(target.creatureTemplate), tracked != null, y);
    }
}
