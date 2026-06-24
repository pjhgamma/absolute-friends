using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static RippleFriends.Core.FriendTracker;

namespace RippleFriends.Core;

internal static class ILUtils
{
    public static void IL_Branch_Ripple(ILContext il)
    {
        ILCursor c = new(il);
        var l = c.DefineLabel();
        TypeReference abstractPhysicalObjectType = il.Module.ImportReference(typeof(AbstractPhysicalObject));
        VariableDefinition abstractPhysicalObject1 = new(abstractPhysicalObjectType);
        VariableDefinition abstractPhysicalObject2 = new(abstractPhysicalObjectType);

        il.Body.Variables.Add(abstractPhysicalObject1);
        il.Body.Variables.Add(abstractPhysicalObject2);

        while (c.TryGotoNext(
            i => i.MatchLdfld<AbstractPhysicalObject>("rippleLayer")
        ))
        {
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Stloc, abstractPhysicalObject1);
            c.Index += 2;

            if (c.TryGotoNext(
                i => i.MatchLdfld<AbstractPhysicalObject>("rippleLayer")
            ))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Stloc, abstractPhysicalObject2);

                if (c.TryGotoNext(
                    MoveType.After,
                    i => i.MatchBrfalse(out l)
                ))
                {
                    c.Emit(OpCodes.Ldloc, abstractPhysicalObject1);
                    c.IncomingLabels.ToList().ForEach(l => l.Target = c.Prev);
                    c.EmitDelegate((AbstractPhysicalObject abstractPhysicalObject) => abstractPhysicalObject.realizedObject);
                    c.Emit(OpCodes.Ldloc, abstractPhysicalObject2);
                    c.EmitDelegate((AbstractPhysicalObject abstractPhysicalObject) => abstractPhysicalObject.realizedObject);
                    c.EmitDelegate(IsFriend);
                    c.Emit(OpCodes.Brtrue, l);
                }
            }
        }
    }

    public static void IL_Return_Creature<T>(ILContext il) where T : PhysicalObject
    {
        ILCursor c = new(il);

        while (c.TryGotoNext(
            MoveType.After,
            i => i.MatchIsinst<Creature>()
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Creature creature, T physicalObject) =>
            {
                if (IsFriend(creature, physicalObject))
                {
                    return null;
                }
                return creature;
            });
        }
    }

    public static void IL_Tongue<T>(ILContext il, Func<T, Creature?> getOwner)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(
            i => i.MatchLdfld<SharedPhysics.CollisionResult>("chunk"),
            i => i.MatchBrfalse(out _)
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((SharedPhysics.CollisionResult collisionResult, T tongue) =>
            {
                if (IsFriend(getOwner(tongue), collisionResult.chunk.owner))
                {
                    return new(null, null, null, false, default);
                }
                return collisionResult;
            });
        }
    }
}
