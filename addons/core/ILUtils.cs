using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using AbsoluteFriends.Diagnostics;

namespace AbsoluteFriends.Core;

public static class ILUtils
{
    private static AbstractPhysicalObject? _abstractPhysicalObject1;

    private static AbstractPhysicalObject? _abstractPhysicalObject2;

    extension(ILContext il)
    {
        public void FriendBranch()
        {
            ILCursor c = new(il);
            ILLabel? l = null;

            while (c.TryGotoNext(
                i => i.MatchLdfld<AbstractPhysicalObject>("rippleLayer")
            ))
            {
                c.Remove();
                c.EmitGuarded((AbstractPhysicalObject abstractPhysicalObject) =>
                {
                    _abstractPhysicalObject1 = abstractPhysicalObject;

                    return abstractPhysicalObject.rippleLayer;
                }, abstractPhysicalObject => abstractPhysicalObject.rippleLayer);

                if (!c.TryGotoNext(
                    i => i.MatchLdfld<AbstractPhysicalObject>("rippleLayer"),
                    i => i.MatchBeq(out l)
                ))
                {
                    return;
                }

                c.Remove();
                c.EmitGuarded((AbstractPhysicalObject abstractPhysicalObject) =>
                {
                    _abstractPhysicalObject2 = abstractPhysicalObject;

                    return abstractPhysicalObject.rippleLayer;
                }, abstractPhysicalObject => abstractPhysicalObject.rippleLayer);

                c.GotoLabel(l, MoveType.AfterLabel);

                if (c.Prev is not { } branch || !branch.MatchBrfalse(out l))
                {
                    continue;
                }

                c.EmitGuarded(() =>
                {
                    bool isFriend = _abstractPhysicalObject1.IsFriend(_abstractPhysicalObject2);

                    _abstractPhysicalObject1 = _abstractPhysicalObject2 = null;

                    return isFriend;
                }, () =>
                {
                    _abstractPhysicalObject1 = _abstractPhysicalObject2 = null;

                    return false;
                });
                c.Emit(OpCodes.Brtrue, l);
            }
        }

        public void NullCreature<T>() where T : PhysicalObject
        {
            ILCursor c = new(il);

            while (c.TryGotoNext(
                MoveType.After,
                i => i.MatchIsinst<Creature>()
            ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitGuarded((Creature creature, T physicalObject) => creature.IsFriend(physicalObject) ? null : creature, (creature, _) => creature);
            }
        }

        public void WeaponHitByWeapon<T>(string name) where T : Weapon => il.WeaponHitBy<T>(name, OpCodes.Ldarg_1);

        public void WeaponHitByExplosion<T>(string name) where T : Weapon => il.WeaponHitBy<T>(name, OpCodes.Ldarg_2);

        public void ScareObject<T>() where T : PhysicalObject
        {
            ILCursor c = new(il);

            while (c.TryGotoNext(
                MoveType.After,
                i => i.MatchNewobj<FirecrackerPlant.ScareObject>()
            ))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitGuarded((FirecrackerPlant.ScareObject scareObject, T physicalObject) => physicalObject.PropagateOwnerTo(scareObject));
            }
        }

        public void MushroomShare()
        {
            ILCursor c = new(il);

            while (c.TryGotoNext(
                i => i.MatchStfld<Player>("mushroomCounter")
            ))
            {
                c.Remove();
                c.EmitGuarded((Player slugcat, int mushroomCounter) =>
                {
                    slugcat.mushroomCounter = mushroomCounter;

                    foreach (var abstractCreature in FriendUtils.TrackedFriendsIncludingPlayers)
                    {
                        if (abstractCreature.realizedCreature is Player player)
                        {
                            player.mushroomCounter = Math.Max(mushroomCounter, player.mushroomCounter);
                        }
                    }
                }, (slugcat, mushroomCounter) => slugcat.mushroomCounter = mushroomCounter);
            }
        }

        public void TongueUpdate<T>(Func<T, Creature?> getOwner)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                i => i.MatchLdfld<SharedPhysics.CollisionResult>("chunk"),
                i => i.MatchBrfalse(out _)
            ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitGuarded(
                    (SharedPhysics.CollisionResult collisionResult, T tongue) => getOwner(tongue).IsFriend(collisionResult.chunk?.owner)
                        ? new(null, null, null, false, default)
                        : collisionResult,
                    (collisionResult, _) => collisionResult
                );
            }
        }

        public void JokeRifleUse<T>() where T : PhysicalObject
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdfld<AbstractPhysicalObject>("realizedObject"),
                i => i.MatchIsinst<T>()
            ))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitGuarded((T? physicalObject, JokeRifle jokeRifle) => physicalObject.SetOwner(jokeRifle.Grabber));
            }
        }

        public void JokeRifleUse(JokeRifle.AbstractRifle.AmmoType ammoType)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdfld<AbstractPhysicalObject>("realizedObject"),
                i => i.MatchIsinst<Bullet>()
            ))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitGuarded((Bullet? bullet, JokeRifle jokeRifle) =>
                {
                    if (jokeRifle.abstractRifle?.ammoStyle == ammoType)
                    {
                        bullet.SetOwner(jokeRifle.Grabber);
                    }
                });
            }
        }

        private void WeaponHitBy<T>(string name, OpCode source) where T : Weapon
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCall<T>(name)
            ))
            {
                c.Emit(source);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitGuarded(OwnerUtils.PropagateOwnerTo);
            }
        }
    }
}
