using MonoMod.Cil;
using MoreSlugcats;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class FireEggHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.FireEgg;

    [HookPatch(typeof(IL.MoreSlugcats.FireEgg), nameof(IL.MoreSlugcats.FireEgg.Update))]
    [HookTest([679, 760, 765], ["brfalse; ldloc.s; ldfld CollisionResult::chunk", "brfalse.s; ldarg.0; call FireEgg::get_stuckInChunk", "ldflda Creature::enteringShortCut"])]
    private static void IL_FireEgg_Update(ILContext il)
    {
        IL_Return_Creature<FireEgg>(il);
    }

    [HookPatch(typeof(IL.MoreSlugcats.FireEgg), nameof(IL.MoreSlugcats.FireEgg.Collide))]
    [HookTest([7], ["brfalse; ldarg.0; ldfld FireEgg::mode"])]
    private static void IL_FireEgg_Collide(ILContext il)
    {
        IL_Return_Creature<FireEgg>(il);
    }
}
