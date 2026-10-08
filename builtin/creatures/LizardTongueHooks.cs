using AbsoluteFriends.Core;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Hooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using Watcher;

namespace AbsoluteFriends.Creatures;

internal class LizardTongueHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.LizardTongue];

    private static bool IsLickTarget(Lizard? lizard, PhysicalObject? target)
    {
        return lizard != null
            && lizard.Template.type == WatcherEnums.CreatureTemplateType.IndigoLizard
            && target is Creature { dead: false, repelLocusts: <= 0 } creature
            && creature.room?.locusts?.GetSwarmProgress(creature) is > 0f;
    }

    [HookPatch(typeof(On.LizardAI), nameof(On.LizardAI.Update))]
    private static void On_LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
    {
        orig(self);

        if (self.lizard.Template.type != WatcherEnums.CreatureTemplateType.IndigoLizard || self.lizard.tongue?.Ready != true || self.lizard.grasps[0] != null || self.lizard.room == null)
        {
            return;
        }

        foreach (var creature in self.lizard.room.PlayerFriends)
        {
            if (
                creature != self.lizard
                && self.lizard.IsFriend(creature)
                && IsLickTarget(self.lizard, creature)
                && Custom.DistLess(self.lizard.mainBodyChunk.pos, creature.mainBodyChunk.pos, self.lizard.lizardParams.tongueAttackRange)
                && (self.lizard.Submersion < 0.5f || creature.Submersion < 0.5f)
                && self.tracker?.RepresentationForCreature(creature.abstractCreature, false) is { VisualContact: true } representation
            )
            {
                self.focusCreature = representation;
                self.lizard.EnterAnimation(Lizard.Animation.ShootTongue, false);

                return;
            }
        }
    }

    [HookPatch(typeof(On.Lizard), nameof(On.Lizard.EnterAnimation))]
    private static void On_Lizard_EnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard self, Lizard.Animation anim, bool forceAnimationChange)
    {
        if (anim == Lizard.Animation.ShootTongue && !self.safariControlled && self.IsFocusFriend && !IsLickTarget(self, self.AI?.focusCreature?.representedCreature?.realizedCreature))
        {
            return;
        }

        orig(self, anim, forceAnimationChange);
    }

    [HookPatch(typeof(IL.LizardTongue), nameof(IL.LizardTongue.Update))]
    [HookTest([414], ["stfld LizardTongue::attached"])]
    private static void IL_LizardTongue_Update(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            MoveType.After,
            i => i.MatchLdfld<SharedPhysics.CollisionResult>("chunk")
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGuarded(
                (BodyChunk? chunk, LizardTongue tongue) => chunk?.owner is not { } owner || !tongue.lizard.IsFriend(owner) || IsLickTarget(tongue.lizard, owner)
                    ? chunk
                    : null,
                (chunk, _) => chunk
            );
        }
    }

    [HookPatch(typeof(On.LizardTongue), nameof(On.LizardTongue.Update))]
    private static void On_LizardTongue_Update(On.LizardTongue.orig_Update orig, LizardTongue self)
    {
        orig(self);

        if (self.attached?.owner is { } owner && self.lizard.IsFriend(owner) && !IsLickTarget(self.lizard, owner))
        {
            self.Retract();
        }
    }
}
