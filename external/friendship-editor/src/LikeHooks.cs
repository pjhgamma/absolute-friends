using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.FriendshipEditor;

internal class LikeHooks : BaseHooks
{
    private const int SweepInterval = 20;

    protected override Configurable<bool>[] Options => [Config.LikeEditor];

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        if (self.clock % SweepInterval == 0)
        {
            RuleEffects.ApplyLikes(self);
        }
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceLike))]
    private static void On_Relationship_InfluenceLike(On.SocialMemory.Relationship.orig_InfluenceLike orig, SocialMemory.Relationship self, float change)
    {
        orig(self, change);
        RuleEffects.ApplyRelationship(self);
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceTempLike))]
    private static void On_Relationship_InfluenceTempLike(On.SocialMemory.Relationship.orig_InfluenceTempLike orig, SocialMemory.Relationship self, float change)
    {
        orig(self, change);
        RuleEffects.ApplyRelationship(self);
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceKnow))]
    private static void On_Relationship_InfluenceKnow(On.SocialMemory.Relationship.orig_InfluenceKnow orig, SocialMemory.Relationship self, float amountOfLerpUpwards)
    {
        orig(self, amountOfLerpUpwards);
        RuleEffects.ApplyRelationship(self);
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.EvenOutTemps))]
    private static void On_Relationship_EvenOutTemps(On.SocialMemory.Relationship.orig_EvenOutTemps orig, SocialMemory.Relationship self, float speed)
    {
        orig(self, speed);
        RuleEffects.ApplyRelationship(self);
    }
}
