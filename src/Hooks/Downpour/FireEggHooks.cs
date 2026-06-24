using MonoMod.Cil;
using MoreSlugcats;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class FireEggHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.FireEgg;

    [HookPatch(typeof(IL.MoreSlugcats.FireEgg), nameof(IL.MoreSlugcats.FireEgg.Update))]
    private static void IL_FireEgg_Update(ILContext il)
    {
        IL_Return_Creature<FireEgg>(il);
    }

    [HookPatch(typeof(IL.MoreSlugcats.FireEgg), nameof(IL.MoreSlugcats.FireEgg.Collide))]
    private static void IL_FireEgg_Collide(ILContext il)
    {
        IL_Return_Creature<FireEgg>(il);
    }
}
