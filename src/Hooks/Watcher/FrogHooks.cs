using Mono.Cecil.Cil;
using MonoMod.Cil;
using static RippleFriends.Core.FriendTracker;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Diagnostics;
using RippleFriends.Options;
using Watcher;

namespace RippleFriends.Hooks.Watcher;

internal class FrogHooks : WatcherHooks
{
    protected override Configurable<bool> Option => Config.Frog;

    [HookPatch(typeof(On.Watcher.Frog), nameof(On.Watcher.Frog.HitThisObject))]
    private static bool On_Frog_HitThisObject(On.Watcher.Frog.orig_HitThisObject orig, Frog self, PhysicalObject obj)
    {
        if (IsFriend(obj, self))
        {
            return false;
        }
        return orig(self, obj);
    }

    [HookPatch(typeof(IL.Watcher.Frog), nameof(IL.Watcher.Frog.ThrowUpdate))]
    [HookTest([58, 68], ["brfalse.s; ldarg.0; ldfld UpdatableAndDeletable::room", "ldloc.2; callvirt SocialEventRecognizer::WeaponAttack"])]
    private static void IL_Frog_ThrowUpdate(ILContext il)
    {
        IL_Return_Creature<Frog>(il);
    }

    [HookPatch(typeof(IL.Watcher.Frog), nameof(IL.Watcher.Frog.Update))]
    [HookTest([52], ["ldarg.0; ldloc.1; callvirt AbstractCreature::get_realizedCreature"])]
    private static void IL_Frog_Update(ILContext il)
    {
        ILCursor c = new(il);
        var l = c.DefineLabel();

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchCallvirt<AbstractCreature>("get_realizedCreature"),
            i => i.MatchCallvirt<Creature>("get_dead"),
            i => i.MatchBrtrue(out l)
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_1);
            c.EmitGuarded<UpdatableAndDeletable?, UpdatableAndDeletable?, bool>(IsFriend);
            c.Emit(OpCodes.Brtrue, l);
        }
    }
}
