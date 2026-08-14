using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class SaintAttunementHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.SaintAttunement;

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ClassMechanicsSaint))]
    [HookTest([1228, 1231, 1241], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldloc.s; callvirt PhysicalObject::get_bodyChunks"])]
    private static void IL_Player_ClassMechanicsSaint(ILContext il)
    {
        IL_Branch_Ripple(il);
    }
}
