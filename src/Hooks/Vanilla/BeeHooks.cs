using Mono.Cecil.Cil;
using MonoMod.Cil;
using static RippleFriends.Core.FriendTracker;
using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Diagnostics;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class BeeHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Bee;

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
            c.EmitGuarded((Creature creauture, SporePlant.Bee bee) =>
            {
                if (IsFriend(creauture, bee))
                {
                    return null;
                }
                return creauture;
            }, (creauture, _) => creauture);
        }
    }

    [HookPatch(typeof(IL.JokeRifle), nameof(IL.JokeRifle.Use))]
    [HookTest([645], ["stloc.s; ldloc.s; ldc.i4.1"])]
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
            c.EmitGuarded((SporePlant.Bee bee, JokeRifle jokeRifle) =>
            {
                SetThrower(bee, GetGrabber(jokeRifle));
            });
        }
    }

    [HookPatch(typeof(On.SporePlant.Bee), nameof(On.SporePlant.Bee.HuntChunkIfPossible))]
    private static bool On_SporePlant_Bee_HuntChunkIfPossible(On.SporePlant.Bee.orig_HuntChunkIfPossible orig, SporePlant.Bee self, BodyChunk potentialHuntChunk)
    {
        if (IsFriend(potentialHuntChunk.owner, self))
        {
            return false;
        }
        return orig(self, potentialHuntChunk);
    }
}
