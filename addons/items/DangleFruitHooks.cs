using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Items;

internal class DownpourDangleFruitHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.DangleFruit];

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([1027], ["ldloc.s; ldarg.0; call JokeRifle::get_firePos"])]
    private static void IL_JokeRifle_Use(ILContext il) => il.JokeRifleUse(JokeRifle.AbstractRifle.AmmoType.Fruit);
}
