using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Players;

internal class CarryHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Carry];

    private static bool IsCarried(Player slugcat, PhysicalObject? obj)
    {
        return obj is Creature { dead: false, Template.smallCreature: false } creature
            && creature is not Player
            && creature.abstractCreature.IsTrackedFor(slugcat.abstractCreature);
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.Grabability))]
    private static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability grabability = orig(self, obj);

        return grabability == Player.ObjectGrabability.CantGrab && IsCarried(self, obj) ? Player.ObjectGrabability.Drag : grabability;
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.IsCreatureLegalToHoldWithoutStun))]
    private static bool On_Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    {
        return IsCarried(self, grabCheck) || orig(self, grabCheck);
    }
}
