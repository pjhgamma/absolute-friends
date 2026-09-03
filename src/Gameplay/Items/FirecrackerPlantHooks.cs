using MonoMod.Cil;
using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Gameplay.Items;

internal class FirecrackerPlantHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FirecrackerPlant];

    [HookPatch(typeof(IL.FirecrackerPlant), nameof(IL.FirecrackerPlant.PopLump))]
    [HookTest([170, 173, 186, 285], ["ldarg.0; ldfld PhysicalObject::abstractPhysicalObject", "beq.s; ldarg.0; ldfld UpdatableAndDeletable::room", "ldarg.0; ldfld UpdatableAndDeletable::room", "newobj ScareObject::.ctor"])]
    private static void IL_FirecrackerPlant_PopLump(ILContext il) => il.BranchRipple();

    [HookPatch(typeof(IL.FirecrackerPlant), nameof(IL.FirecrackerPlant.HitByExplosion))]
    [HookTest([8], ["ldarg.0; call FirecrackerPlant::Ignite"])]
    private static void IL_FirecrackerPlant_HitByExplosion(ILContext il) => il.WeaponHitByExplosion<FirecrackerPlant>("Ignite");
}

internal class DownpourFirecrackerPlantHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.FirecrackerPlant];

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([367, 370, 383, 488], ["ldarg.0; ldfld PhysicalObject::abstractPhysicalObject", "beq.s; ldarg.0; ldfld UpdatableAndDeletable::room", "ldarg.0; ldfld UpdatableAndDeletable::room", "newobj ScareObject::.ctor"])]
    private static void IL_JokeRifle_Use(ILContext il) => il.BranchRipple();
}
