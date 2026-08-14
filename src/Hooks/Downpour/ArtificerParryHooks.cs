using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class ArtificerParryHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.ArtificerParry;

    [HookPatch(typeof(IL.Player), nameof(IL.Player.ClassMechanicsArtificer))]
    [HookTest([891, 894, 910], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldarg.0; ldfld UpdatableAndDeletable::room"])]
    private static void IL_Player_ClassMechanicsArtificer(ILContext il)
    {
        IL_Branch_Ripple(il);
    }
}
