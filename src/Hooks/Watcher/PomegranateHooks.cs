using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Watcher;

internal class PomegranateHooks : WatcherHooks
{
    protected override Configurable<bool> Option => Config.Pomegranate;

    [HookPatch(typeof(IL.Pomegranate), nameof(IL.Pomegranate.Collide))]
    [HookTest([7, 58, 135], ["brfalse; ldarg.0; call Pomegranate::get_falling", "ldarg.0; ldfld Pomegranate::killTagHolder", "ldarg.0; call PhysicalObject::get_firstChunk"])]
    private static void IL_Pomegranate_Collide(ILContext il)
    {
        IL_Return_Creature<Pomegranate>(il);
    }
}
