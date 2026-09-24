using Mono.Cecil.Cil;
using MonoMod.Cil;
using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriends.Items;

internal class SporePlantHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.SporePlant];

    [HookPatch(typeof(On.SporePlant), nameof(On.SporePlant.Thrown))]
    private static void On_SporePlant_Thrown(On.SporePlant.orig_Thrown orig, SporePlant self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        self.bees.ForEach(self.PropagateOwnerTo);

        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
    }

    [HookPatch(typeof(On.SporePlant.Bee), nameof(On.SporePlant.Bee.ctor))]
    private static void On_SporePlant_Bee_ctor(On.SporePlant.Bee.orig_ctor orig, SporePlant.Bee self, SporePlant owner, bool angry, Vector2 pos, Vector2 vel, SporePlant.Bee.Mode initMode)
    {
        owner.PropagateOwnerTo(self);

        orig(self, owner, angry, pos, vel, initMode);
    }

    [HookPatch(typeof(IL.SporePlant), nameof(IL.SporePlant.HitByWeapon))]
    [HookTest([6], ["ldarg.0; call SporePlant::BeeTrigger"])]
    private static void IL_SporePlant_HitByWeapon(ILContext il) => il.WeaponHitByWeapon<SporePlant>("BeeTrigger");

    [HookPatch(typeof(IL.SporePlant), nameof(IL.SporePlant.HitByExplosion))]
    [HookTest([8], ["ldarg.0; call SporePlant::BeeTrigger"])]
    private static void IL_SporePlant_HitByExplosion(ILContext il) => il.WeaponHitByExplosion<SporePlant>("BeeTrigger");

    [HookPatch(typeof(IL.SporePlant.Bee), nameof(IL.SporePlant.Bee.LookForRandomCreatureToHunt))]
    [HookTest([44], ["brfalse; ldloc.0; callvirt AbstractCreature::get_realizedCreature"])]
    private static void IL_SporePlant_Bee_LookForRandomCreatureToHunt(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchCallvirt<AbstractCreature>("get_realizedCreature")
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((Creature creature, SporePlant.Bee bee) => creature.IsFriend(bee.Owner) ? null : creature, (creature, _) => creature);
        }
    }

    [HookPatch(typeof(On.SporePlant.Bee), nameof(On.SporePlant.Bee.HuntChunkIfPossible))]
    private static bool On_SporePlant_Bee_HuntChunkIfPossible(On.SporePlant.Bee.orig_HuntChunkIfPossible orig, SporePlant.Bee self, BodyChunk potentialHuntChunk)
    {
        return !potentialHuntChunk.owner.IsFriend(self.Owner) && orig(self, potentialHuntChunk);
    }

    [HookPatch(typeof(On.SporePlant), nameof(On.SporePlant.Update))]
    private static void On_SporePlant_Update(On.SporePlant.orig_Update orig, SporePlant self, bool eu)
    {
        orig(self, eu);

        if (self.attachedBees.All(bee => bee.life < 0))
        {
            self.attachedBees.ForEach(bee => bee.Destroy());
        }
    }
}

internal class DownpourSporePlantHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.SporePlant];

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([655], ["stloc.s; ldloc.s; ldc.i4.1"])]
    private static void IL_JokeRifle_Use(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchNewobj<SporePlant.Bee>()
        ))
        {
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded((SporePlant.Bee bee, JokeRifle jokeRifle) => bee.SetOwner(jokeRifle.Grabber));
        }
    }
}
