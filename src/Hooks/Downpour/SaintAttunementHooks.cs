using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class SaintAttunementHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.SaintAttunement;

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ClassMechanicsSaint))]
    private static void IL_Player_ClassMechanicsSaint(ILContext il)
    {
        IL_Branch_Ripple(il);
    }
}
