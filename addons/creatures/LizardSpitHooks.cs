using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Creatures;

internal class LizardSpitHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.LizardSpit];

    [HookPatch(typeof(IL.LizardSpit), nameof(IL.LizardSpit.Update))]
    [HookTest([693], ["ldfld CollisionResult::chunk"])]
    private static void IL_LizardSpit_Update(ILContext il) => il.TongueUpdate<LizardSpit>(lizardSpit => lizardSpit.lizard);

    [HookPatch(typeof(On.Lizard), nameof(On.Lizard.EnterAnimation))]
    private static void On_Lizard_EnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard self, Lizard.Animation anim, bool forceAnimationChange)
    {
        if (anim == Lizard.Animation.Spit && !self.safariControlled && self.IsFriend(self.AI?.preyTracker?.MostAttractivePrey?.representedCreature))
        {
            return;
        }

        orig(self, anim, forceAnimationChange);
    }
}
