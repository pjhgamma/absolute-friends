using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class SingularityBombHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.SingularityBomb;

    [HookPatch(typeof(IL.MoreSlugcats.SingularityBomb), nameof(IL.MoreSlugcats.SingularityBomb.Update))]
    [HookTest([490, 493, 509, 660, 663, 679], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldc.i4.0; stloc.s; br", "ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldc.i4.0; stloc.s; br"])]
    private static void IL_SingularityBomb_Update(ILContext il)
    {
        IL_Branch_Ripple(il);
    }

    [HookPatch(typeof(IL.MoreSlugcats.SingularityBomb), nameof(IL.MoreSlugcats.SingularityBomb.Explode))]
    [HookTest([341, 344, 360], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldarg.0; ldfld UpdatableAndDeletable::room"])]
    private static void IL_SingularityBomb_Explode(ILContext il)
    {
        IL_Branch_Ripple(il);
    }
}
