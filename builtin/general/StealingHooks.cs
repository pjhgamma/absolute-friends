using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.General;

internal class StealingHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Stealing];

    private static bool CanSteal(Creature self, PhysicalObject obj)
    {
        return (self.IsPlayer && (obj.IsSlugcat || obj.Grabber.IsSlugpup)) || !self.IsFriend(obj.Grabber);
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Grab))]
    private static bool On_Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        return CanSteal(self, obj) && orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        return CanSteal(self, obj) && orig(self, obj);
    }
}
