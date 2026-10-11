using AbsoluteFriends.Manager;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class LikeView(FriendTab tab, RainWorldGame game, float y) : CreatureView(tab, game, y, Stat.Like, Stat.TempLike, Stat.Know)
{
    internal override string Title => "Like";

    internal static bool IsAvailableFor(AbstractCreature friend) => Config.LikeEditor.IsActive && IsFriendTarget(friend);

    internal override bool IsVisibleFor(AbstractCreature friend) => IsAvailableFor(friend);

    protected override bool Knows(AbstractCreature creature, AbstractCreature target) => creature.state?.socialMemory?.GetRelationship(target.ID) != null;

    protected override void ShowRow(CreatureRow row, AbstractCreature creature, AbstractCreature target, float y) =>
        row.ShowLikes(creature, creature.state?.socialMemory?.GetRelationship(target.ID) is { } memory ? (memory.like, memory.tempLike, memory.know) : null, y);

    protected override void Apply() => RuleEffects.ApplyLikes(Game);
}
