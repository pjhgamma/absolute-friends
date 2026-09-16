using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Players;

internal class SaintAttunementHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.SaintAttunement];

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ClassMechanicsSaint))]
    [HookTest([1229, 1232, 1241], ["ldarg.0; ldfld PhysicalObject::abstractPhysicalObject", "beq.s; ldloc.s; ldfld PhysicalObject::abstractPhysicalObject", "ldloc.s; callvirt PhysicalObject::get_bodyChunks"])]
    private static void IL_Player_ClassMechanicsSaint(ILContext il) => il.RippleBranch();
}
