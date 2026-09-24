using Mono.Cecil.Cil;
using MonoMod.Cil;
using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using RWCustom;
using UnityEngine;
using Watcher;

namespace AbsoluteFriends.Items;

internal class FrogHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.Frog];

    [HookPatch(typeof(On.Watcher.Frog), nameof(On.Watcher.Frog.Thrown))]
    private static void On_Frog_Thrown(On.Watcher.Frog.orig_Thrown orig, Frog self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        self.SetOwner();
        self.SetOwner(thrownBy);

        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
    }

    [HookPatch(typeof(On.Watcher.Frog), nameof(On.Watcher.Frog.ThrowUpdate))]
    private static void On_Frog_ThrowUpdate(On.Watcher.Frog.orig_ThrowUpdate orig, Frog self)
    {
        orig(self);

        if (self.thrownBy == null && self.Grabber == null)
        {
            self.SetOwner();
        }
    }

    [HookPatch(typeof(On.Watcher.Frog), nameof(On.Watcher.Frog.HitThisObject))]
    private static bool On_Frog_HitThisObject(On.Watcher.Frog.orig_HitThisObject orig, Frog self, PhysicalObject obj)
    {
        return !obj.IsFriend(self) && orig(self, obj);
    }

    [HookPatch(typeof(IL.Watcher.Frog), nameof(IL.Watcher.Frog.ThrowUpdate))]
    [HookTest([58, 68], ["brfalse.s; ldarg.0; ldfld UpdatableAndDeletable::room", "ldloc.2; callvirt SocialEventRecognizer::WeaponAttack"])]
    private static void IL_Frog_ThrowUpdate(ILContext il) => il.NullCreature<Frog>();

    [HookPatch(typeof(IL.Watcher.Frog), nameof(IL.Watcher.Frog.Update))]
    [HookTest([52], ["ldarg.0; ldloc.1; callvirt AbstractCreature::get_realizedCreature"])]
    private static void IL_Frog_Update(ILContext il)
    {
        ILCursor c = new(il);
        ILLabel l = c.DefineLabel();

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchCallvirt<AbstractCreature>("get_realizedCreature"),
            i => i.MatchCallvirt<Creature>("get_dead"),
            i => i.MatchBrtrue(out l)
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_1);
            c.EmitGuarded((Frog frog, AbstractCreature abstractCreature) => frog.IsFriend(abstractCreature));
            c.Emit(OpCodes.Brtrue, l);
        }
    }
}
