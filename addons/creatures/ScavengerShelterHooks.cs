using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Creatures;

internal class ScavengerShelterHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.ScavengerShelter];

    private static bool IsShelterFriend(Scavenger? scavenger)
    {
        return scavenger?.room?.abstractRoom?.shelter == true && scavenger.IsFriendOfPlayer;
    }

    [HookPatch(typeof(On.Scavenger), nameof(On.Scavenger.PickUpAndPlaceInInventory))]
    private static void On_Scavenger_PickUpAndPlaceInInventory(On.Scavenger.orig_PickUpAndPlaceInInventory orig, Scavenger self, PhysicalObject obj, bool lethalityBypass)
    {
        if (!IsShelterFriend(self) || obj?.abstractPhysicalObject == self.AI?.giftForMe)
        {
            orig(self, obj, lethalityBypass);
        }
    }

    [HookPatch(typeof(On.ScavengerAI), nameof(On.ScavengerAI.PickUpItemScore))]
    private static float On_ScavengerAI_PickUpItemScore(On.ScavengerAI.orig_PickUpItemScore orig, ScavengerAI self, ItemTracker.ItemRepresentation rep)
    {
        return IsShelterFriend(self.scavenger) ? float.MinValue : orig(self, rep);
    }
}
