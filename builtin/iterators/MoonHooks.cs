using AbsoluteFriends.Hooks;
using UnityEngine;

namespace AbsoluteFriends.Iterators;

internal class MoonHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Moon];

    [HookPatch(typeof(On.SLOrcacleState), nameof(On.SLOrcacleState.InfluenceLike))]
    private static void On_SLOrcacleState_InfluenceLike(On.SLOrcacleState.orig_InfluenceLike orig, SLOrcacleState self, float influence)
    {
        orig(self, Mathf.Max(influence, 0f));
    }

    [HookPatch(typeof(On.SLOracleBehaviorHasMark), nameof(On.SLOracleBehaviorHasMark.NoLongerOnSpeakingTerms))]
    private static void On_SLOracleBehaviorHasMark_NoLongerOnSpeakingTerms(On.SLOracleBehaviorHasMark.orig_NoLongerOnSpeakingTerms orig, SLOracleBehaviorHasMark self)
    {
    }

    [HookPatch(typeof(On.SLOracleBehaviorHasMark), nameof(On.SLOracleBehaviorHasMark.Update))]
    private static void On_SLOracleBehaviorHasMark_Update(On.SLOracleBehaviorHasMark.orig_Update orig, SLOracleBehaviorHasMark self, bool eu)
    {
        SLOrcacleState state = self.State;
        int annoyances = state.annoyances;
        int interruptions = state.totalInterruptions;
        bool hasTold = state.hasToldPlayerNotToEatNeurons;
        bool increasesLike = state.increaseLikeOnSave;

        orig(self, eu);

        state.annoyances = Mathf.Min(state.annoyances, annoyances);
        state.totalInterruptions = Mathf.Min(state.totalInterruptions, interruptions);
        state.hasToldPlayerNotToEatNeurons &= hasTold;
        state.increaseLikeOnSave |= increasesLike;
    }
}
