using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class ArtificerParryHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.ArtificerParry;

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ClassMechanicsArtificer))]
    private static void IL_Player_ClassMechanicsArtificer(ILContext il)
    {
        IL_Branch_Ripple(il);
    }
}
