using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Items;

internal class ExplosiveSpearHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.ExplosiveSpear];

    [HookPatch(typeof(IL.ExplosiveSpear), nameof(IL.ExplosiveSpear.HitByExplosion))]
    [HookTest([8], ["ldarg.0; call ExplosiveSpear::Ignite"])]
    private static void IL_ExplosiveSpear_HitByExplosion(ILContext il) => il.WeaponHitByExplosion<ExplosiveSpear>("Ignite");
}
