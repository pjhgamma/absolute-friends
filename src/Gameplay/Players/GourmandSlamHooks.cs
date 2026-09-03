using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Diagnostics;
using RippleFriends.Friends;
using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Gameplay.Players;

internal class GourmandSlamHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.GourmandSlam];

    [HookPatch(typeof(IL.Player), nameof(IL.Player.Collide))]
    [HookTest([134], ["ldarg.0; call Player::get_isGourmand"])]
    private static void IL_Player_Collide(ILContext il)
    {
        ILCursor c = new(il);
        ILLabel l = c.DefineLabel();

        if (c.TryGotoNext(
            i => i.MatchIsinst<Creature>()
        ) && c.TryGotoNext(
            i => i.MatchLdsfld<ModManager>("MSC"),
            i => i.MatchBrfalse(out l)
        ) && c.TryGotoPrev(
            i => i.MatchLdarg(0),
            i => i.MatchCall<Player>("get_isGourmand"),
            i => i.MatchBrtrue(out _)
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitGuarded((PhysicalObject source, PhysicalObject target) => source.IsFriend(target));
            c.Emit(OpCodes.Brtrue, l);
        }
    }
}
