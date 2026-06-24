using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class PebblesHook : BaseHooks
{
    protected override Configurable<bool> Option => Config.Pebbles;

    [HookPatch(typeof(IL.SSOracleBehavior.ThrowOutBehavior), nameof(IL.SSOracleBehavior.ThrowOutBehavior.Update))]
    private static void IL_ThrowOutBehavior_Update(ILContext il)
    {
        ILCursor c = new(il);
        var l = c.DefineLabel();

        if (c.TryGotoNext(
            i => i.MatchLdarg(0),
            i => i.MatchCall<SSOracleBehavior.SubBehavior>("get_player"),
            i => i.MatchCallvirt<Creature>("get_dead"),
            i => i.MatchBrfalse(out _),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<SSOracleBehavior.SubBehavior>("owner"),
            i => i.MatchLdfld<SSOracleBehavior>("killFac"),
            i => i.MatchLdcR4(0.5f),
            i => i.MatchBleUn(out l)
        ))
        {
            c.Emit(OpCodes.Br, l);
        }
    }
}
