using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.FriendshipEditor;

internal class ReputationHooks : BaseHooks
{
    private const int SweepInterval = 20;

    protected override Configurable<bool>[] Options => [Config.ReputationEditor];

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        if (self.clock % SweepInterval == 0)
        {
            RuleEffects.ApplyReputation(self);
        }
    }

    [HookPatch(typeof(On.CreatureCommunities), nameof(On.CreatureCommunities.InfluenceCell))]
    private static void On_CreatureCommunities_InfluenceCell(On.CreatureCommunities.orig_InfluenceCell orig, CreatureCommunities self, int comm, int reg, int plr, float infl)
    {
        orig(self, comm, reg, plr, infl);

        if (self.session?.game is { } game)
        {
            RuleEffects.ApplyReputation(game, self, comm, reg, plr);
        }
    }
}
