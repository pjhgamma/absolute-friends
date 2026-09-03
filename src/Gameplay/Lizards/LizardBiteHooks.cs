using RippleFriends.Friends;
using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Gameplay.Lizards;

internal class LizardBiteHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.LizardBite];

    [HookPatch(typeof(On.LizardAI), nameof(On.LizardAI.DoIWantToBiteThisCreature))]
    private static bool On_LizardAI_DoIWantToBiteThisCreature(On.LizardAI.orig_DoIWantToBiteThisCreature orig, LizardAI self, global::Tracker.CreatureRepresentation otherCrit)
    {
        return !self.creature.IsFriend(otherCrit?.representedCreature) && orig(self, otherCrit);
    }

    [HookPatch(typeof(On.Lizard), nameof(On.Lizard.Bite))]
    private static void On_Lizard_Bite(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
    {
        if (self.IsFriend(chunk?.owner))
        {
            return;
        }

        orig(self, chunk);
    }

    [HookPatch(typeof(On.Lizard), nameof(On.Lizard.DamageAttack))]
    private static void On_Lizard_DamageAttack(On.Lizard.orig_DamageAttack orig, Lizard self, BodyChunk chunk, float dmgFac)
    {
        if (self.IsFriend(chunk?.owner))
        {
            return;
        }

        orig(self, chunk, dmgFac);
    }

    [HookPatch(typeof(On.Lizard), nameof(On.Lizard.EnterAnimation))]
    private static void On_Lizard_EnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard self, Lizard.Animation anim, bool forceAnimationChange)
    {
        if ((anim == Lizard.Animation.PrepareToLounge || anim == Lizard.Animation.Lounge) && self.IsFocusFriend)
        {
            return;
        }

        orig(self, anim, forceAnimationChange);
    }
}
