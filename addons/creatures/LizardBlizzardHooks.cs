using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Watcher;

namespace AbsoluteFriends.Creatures;

internal class LizardBlizzardHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.LizardBlizzard];

    [HookPatch(typeof(IL.Watcher.LizardBlizzardModule), nameof(IL.Watcher.LizardBlizzardModule.Update))]
    [HookTest([164], ["ldarg.0; ldfld LizardBlizzardModule::lizard"])]
    private static void IL_LizardBlizzardModule_Update(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<LizardBlizzardModule>("lizard"),
            i => i.MatchBeq(out _)
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded(
                (PhysicalObject? physicalObject, LizardBlizzardModule module) => module.lizard.IsFriend(physicalObject) ? module.lizard : physicalObject,
                (physicalObject, _) => physicalObject
            );
        }
    }
}
