using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class SnailHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Snail;

    [HookPatch(typeof(IL.Snail), nameof(IL.Snail.Click))]
    [HookTest([366, 369, 379], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldloc.s; callvirt PhysicalObject::get_bodyChunks"])]
    private static void IL_Snail_Click(ILContext il)
    {
        IL_Branch_Ripple(il);
    }

    [HookPatch(typeof(On.Snail), nameof(On.Snail.Click))]
    private static void On_Snail_Click(On.Snail.orig_Click orig, Snail self)
    {
        orig(self);

        if (self.triggerTicker <= 0)
        {
            SetThrower(self);
        }
    }

    [HookPatch(typeof(On.Player), nameof(On.Player.TossObject))]
    private static void On_Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp]?.grabbed is Snail snail)
        {
            SetThrower(snail, self);
        }

        orig(self, grasp, eu);
    }

    [HookPatch(typeof(On.PhysicalObject), nameof(On.PhysicalObject.HitByWeapon))]
    private static void On_PhysicalObject_HitByWeapon(On.PhysicalObject.orig_HitByWeapon orig, PhysicalObject self, Weapon weapon)
    {
        if (self is Snail)
        {
            ChainOwner(weapon, self);
        }

        orig(self, weapon);
    }

    [HookPatch(typeof(On.Snail), nameof(On.Snail.Die))]
    private static void On_Snail_Die(On.Snail.orig_Die orig, Snail self)
    {
        if (self.killTag?.realizedCreature is Player player)
        {
            SetThrower(self, player);
        }

        orig(self);
    }
}
