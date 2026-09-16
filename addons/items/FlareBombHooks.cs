using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Items;

internal class FlareBombHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.FlareBomb];

    [HookPatch(typeof(IL.FlareBomb), nameof(IL.FlareBomb.Update))]
    [HookTest([95, 98, 111], ["ldarg.0; ldfld PhysicalObject::abstractPhysicalObject", "beq.s; ldarg.0; ldfld UpdatableAndDeletable::room", "ldarg.0; call PhysicalObject::get_firstChunk"])]
    private static void IL_FlareBomb_Update(ILContext il) => il.RippleBranch();
}

internal class DownpourFlareBombHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.FlareBomb];

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([1042], ["dup; ldarg.0; ldc.i4"])]
    private static void IL_JokeRifle_Use(ILContext il) => il.JokeRifleUse(JokeRifle.AbstractRifle.AmmoType.Light);
}
