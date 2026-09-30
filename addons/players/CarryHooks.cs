using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.Players;

internal class CarryHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Carry];

    private static bool CanCarry(Player slugcat, PhysicalObject? obj)
    {
        return obj is Creature { dead: false, Template.smallCreature: false } and not Player
            && obj.abstractPhysicalObject is AbstractCreature abstractCreature
            && abstractCreature.IsTrackedFor(slugcat.abstractCreature);
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.Grabability))]
    private static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability grabability = orig(self, obj);

        return grabability is Player.ObjectGrabability.CantGrab or Player.ObjectGrabability.Drag && CanCarry(self, obj)
            ? Player.ObjectGrabability.TwoHands
            : grabability;
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.IsCreatureLegalToHoldWithoutStun))]
    private static bool On_Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    {
        return CanCarry(self, grabCheck) || orig(self, grabCheck);
    }

    [HookPatch(typeof(On.Creature), nameof(On.Creature.Grab))]
    private static bool On_Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (self is Player slugcat && CanCarry(slugcat, obj))
        {
            pacifying = Config.CarryStun.IsActive;
        }

        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }
}
