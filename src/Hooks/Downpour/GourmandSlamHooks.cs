using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class GourmandSlamHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.GourmandSlam;

    [HookPatch(typeof(IL.Player), nameof(IL.Player.Collide))]
    private static void IL_Player_Collide(ILContext il)
    {
        ILCursor c = new(il);
        var l = c.DefineLabel();

        if (c.TryGotoNext(
            i => i.MatchIsinst<Creature>()
        ) && c.TryGotoNext(
            i => i.MatchLdfld<ModManager>("MSC"),
            i => i.MatchBrfalse(out l)
        ))
        {
            c.Emit(OpCodes.Br, l);
        }
    }
}
