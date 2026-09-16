using Mono.Cecil.Cil;
using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Diagnostics;
using RippleFriends.Hooks;

namespace RippleFriends.Items;

internal class PomegranateHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.Pomegranate];

    [HookPatch(typeof(IL.Pomegranate), nameof(IL.Pomegranate.Grabbed))]
    [HookTest([21], ["stfld Pomegranate::killTagHolder"])]
    private static void IL_Pomegranate_Grabbed(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            i => i.MatchStfld<Pomegranate>("killTagHolder")
        ))
        {
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((Player player, Pomegranate pomegranate) => pomegranate.SetOwner(player));
        }
    }

    [HookPatch(typeof(IL.Pomegranate), nameof(IL.Pomegranate.Collide))]
    [HookTest([7, 58, 135], ["brfalse; ldarg.0; call Pomegranate::get_falling", "ldarg.0; ldfld Pomegranate::killTagHolder", "ldarg.0; call PhysicalObject::get_firstChunk"])]
    private static void IL_Pomegranate_Collide(ILContext il) => il.NullCreature<Pomegranate>();
}
