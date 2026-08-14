using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using static RippleFriends.Core.OwnerTracker;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class TubeWormHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.TubeWorm;

    [HookPatch(typeof(IL.TubeWorm.Tongue), nameof(IL.TubeWorm.Tongue.Update))]
    [HookTest([79], ["ldfld CollisionResult::chunk"])]
    private static void IL_TubeWorm_Tongue_Update(ILContext il)
    {
        IL_Tongue<TubeWorm.Tongue>(
            il,
            tongue => GetGrabber(tongue.worm)
        );
    }
}
