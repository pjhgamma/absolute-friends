using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Diagnostics;
using RippleFriends.Hooks;

namespace RippleFriends.Creatures;

internal class LizardPoisonHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.LizardPoison];

    [HookPatch(typeof(IL.Lizard), nameof(IL.Lizard.Update))]
    [HookTest([1278], ["stloc.s; ldloc.s; brfalse.s"])]
    private static void IL_Lizard_Update(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchIsinst<Player>()
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((Player? player, Lizard lizard) => lizard.IsFriend(player) ? null : player, (player, _) => player);
        }
    }

    [HookPatch(typeof(IL.Lizard), nameof(IL.Lizard.Collide))]
    [HookTest([149], ["stloc.2; ldloc.2; brfalse"])]
    private static void IL_Lizard_Collide(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            i => i.MatchCall<Lizard>("get_BasiliskMushroomField"),
            i => i.MatchLdcR4(1f),
            i => i.MatchBneUn(out _)
        ) && c.TryGotoNext(
            MoveType.After,
            i => i.MatchIsinst<Creature>()
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((Creature? creature, Lizard lizard) => lizard.IsFriend(creature) ? null : creature, (creature, _) => creature);
        }
    }
}
