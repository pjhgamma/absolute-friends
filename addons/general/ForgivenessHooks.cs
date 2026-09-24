using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using UnityEngine;

namespace AbsoluteFriends.General;

internal class ForgivenessHooks : BaseHooks
{
    private static bool _isSparing;

    protected override Configurable<bool>[] Options => [Config.Forgiveness];

    private static void Spare(Action action)
    {
        bool wasSparing = _isSparing;

        _isSparing = true;

        try
        {
            action();
        }
        finally
        {
            _isSparing = wasSparing;
        }
    }

    private static float Spare(float change)
    {
        return _isSparing && change < 0f ? Mathf.Lerp(change, 0f, Config.ForgivenessRatio.Value) : change;
    }

    [HookPatch(typeof(On.SocialEventRecognizer), nameof(On.SocialEventRecognizer.SocialEvent))]
    private static void On_SocialEventRecognizer_SocialEvent(On.SocialEventRecognizer.orig_SocialEvent orig, SocialEventRecognizer self, SocialEventRecognizer.EventID ID, Creature subjectCreature, Creature objectCreature, PhysicalObject involvedItem)
    {
        if (ID == SocialEventRecognizer.EventID.ItemOffering || ID == SocialEventRecognizer.EventID.ItemTransaction || !subjectCreature.IsFriend(objectCreature))
        {
            orig(self, ID, subjectCreature, objectCreature, involvedItem);

            return;
        }

        if (Config.ForgivenessRatio.Value >= 1f)
        {
            return;
        }

        Spare(() => orig(self, ID, subjectCreature, objectCreature, involvedItem));
    }

    [HookPatch(typeof(On.SocialEventRecognizer), nameof(On.SocialEventRecognizer.Killing))]
    private static void On_SocialEventRecognizer_Killing(On.SocialEventRecognizer.orig_Killing orig, SocialEventRecognizer self, Creature killer, Creature victim)
    {
        if (!killer.IsFriend(victim))
        {
            orig(self, killer, victim);

            return;
        }

        Spare(() => orig(self, killer, victim));
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceLike))]
    private static void On_Relationship_InfluenceLike(On.SocialMemory.Relationship.orig_InfluenceLike orig, SocialMemory.Relationship self, float change)
    {
        orig(self, Spare(change));
    }

    [HookPatch(typeof(On.SocialMemory.Relationship), nameof(On.SocialMemory.Relationship.InfluenceTempLike))]
    private static void On_Relationship_InfluenceTempLike(On.SocialMemory.Relationship.orig_InfluenceTempLike orig, SocialMemory.Relationship self, float change)
    {
        orig(self, Spare(change));
    }

    [HookPatch(typeof(On.CreatureCommunities), nameof(On.CreatureCommunities.InfluenceLikeOfPlayer))]
    private static void On_CreatureCommunities_InfluenceLikeOfPlayer(On.CreatureCommunities.orig_InfluenceLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commID, int region, int playerNumber, float influence, float interRegionBleed, float interCommunityBleed)
    {
        orig(self, commID, region, playerNumber, Spare(influence), interRegionBleed, interCommunityBleed);
    }
}
