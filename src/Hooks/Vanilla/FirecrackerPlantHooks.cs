using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class FirecrackerPlantHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.FirecrackerPlant;

    [HookPatch(typeof(IL.FirecrackerPlant), nameof(IL.FirecrackerPlant.PopLump))]
    [HookTest([169, 172, 186, 284], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldarg.0; ldfld UpdatableAndDeletable::room", "ldfld AbstractPhysicalObject::rippleLayer"])]
    private static void IL_FirecrackerPlant_PopLump(ILContext il)
    {
        IL_Branch_Ripple(il);
    }

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([366, 369, 383, 487], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldarg.0; ldfld UpdatableAndDeletable::room", "ldfld AbstractPhysicalObject::rippleLayer"])]
    private static void IL_JokeRifle_Use(ILContext il)
    {
        IL_Branch_Ripple(il);
    }

    [HookPatch(typeof(On.FirecrackerPlant), nameof(On.FirecrackerPlant.HitByExplosion))]
    private static void On_FirecrackerPlant_HitByExplosion(On.FirecrackerPlant.orig_HitByExplosion orig, FirecrackerPlant self, float hitFac, Explosion explosion, int hitChunk)
    {
        ChainOwner(explosion, self);

        orig(self, hitFac, explosion, hitChunk);
    }
}
