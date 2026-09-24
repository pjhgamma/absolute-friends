using MonoMod.Cil;
using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.General;

internal class ExplosionHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Explosion];

    [HookPatch(typeof(On.Explosion), nameof(On.Explosion.ctor))]
    private static void On_Explosion_ctor(On.Explosion.orig_ctor orig, Explosion self, Room room, PhysicalObject sourceObject, UnityEngine.Vector2 pos, int lifeTime, float rad, float force, float damage, float stun, float deafen, Creature killTagHolder, float killTagHolderDmgFactor, float minStun, float backgroundNoise)
    {
        if (killTagHolder != null)
        {
            self.SetOwner(killTagHolder);
        }
        else
        {
            sourceObject.PropagateOwnerTo(self);
        }

        orig(self, room, sourceObject, pos, lifeTime, rad, force, damage, stun, deafen, killTagHolder, killTagHolderDmgFactor, minStun, backgroundNoise);
    }

    [HookPatch(typeof(IL.Explosion), nameof(IL.Explosion.Update))]
    [HookTest([112, 121, 137], ["ldarg.0; ldfld UpdatableAndDeletable::room", "beq.s; ldarg.0; ldfld Explosion::sourceObject", "ldarg.0; ldfld UpdatableAndDeletable::room"])]
    private static void IL_Explosion_Update(ILContext il) => il.FriendBranch();
}
