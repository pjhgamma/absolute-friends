using Mono.Cecil.Cil;
using MonoMod.Cil;
using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using Watcher;

namespace AbsoluteFriends.Creatures;

internal class LizardBeamHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.LizardBeam];

    [HookPatch(typeof(IL.Watcher.LizardBlizzardModule), nameof(IL.Watcher.LizardBlizzardModule.Update))]
    [HookTest([714], ["brfalse; ldarg.0; ldfld LizardBlizzardModule::lizard"])]
    private static void IL_LizardBlizzardModule_Update(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchCallvirt<AbstractCreature>("get_realizedCreature")
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((Creature? creature, LizardBlizzardModule module) => module.lizard.IsFriend(creature) ? null : creature, (creature, _) => creature);
        }
    }
}
