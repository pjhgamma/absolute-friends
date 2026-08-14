using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class ExplosionHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Explosion;

    [HookPatch(typeof(IL.Explosion), nameof(IL.Explosion.Update))]
    [HookTest([111, 120, 137], ["ldfld AbstractPhysicalObject::rippleLayer", "ldfld AbstractPhysicalObject::rippleLayer", "ldarg.0; ldfld UpdatableAndDeletable::room"])]
    private static void IL_Explosion_Update(ILContext il)
    {
        IL_Branch_Ripple(il);
    }

    [HookPatch(typeof(On.PhysicalObject), nameof(On.PhysicalObject.HitByWeapon))]
    private static void On_PhysicalObject_HitByWeapon(On.PhysicalObject.orig_HitByWeapon orig, PhysicalObject self, Weapon weapon)
    {
        if (self is Weapon)
        {
            ChainOwner(weapon, self);
        }

        orig(self, weapon);
    }

    [HookPatch(typeof(On.PhysicalObject), nameof(On.PhysicalObject.HitByExplosion))]
    private static void On_PhysicalObject_HitByExplosion(On.PhysicalObject.orig_HitByExplosion orig, PhysicalObject self, float hitFac, Explosion explosion, int hitChunk)
    {
        if (self is Weapon)
        {
            ChainOwner(explosion, self);
        }

        orig(self, hitFac, explosion, hitChunk);
    }

    [HookPatch(typeof(On.Explosion), nameof(On.Explosion.ctor))]
    private static void On_Explosion_ctor(On.Explosion.orig_ctor orig, Explosion self, Room room, PhysicalObject sourceObject, UnityEngine.Vector2 pos, int lifeTime, float rad, float force, float damage, float stun, float deafen, Creature killTagHolder, float killTagHolderDmgFactor, float minStun, float backgroundNoise)
    {
        ChainOwner(sourceObject, self);

        orig(self, room, sourceObject, pos, lifeTime, rad, force, damage, stun, deafen, killTagHolder, killTagHolderDmgFactor, minStun, backgroundNoise);
    }
}
