using MonoMod.Cil;
using RippleFriends.Friends;
using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Gameplay.Items;

internal class TubeWormHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.TubeWorm];

    [HookPatch(typeof(IL.TubeWorm.Tongue), nameof(IL.TubeWorm.Tongue.Update))]
    [HookTest([79], ["ldfld CollisionResult::chunk"])]
    private static void IL_TubeWorm_Tongue_Update(ILContext il) => il.TongueUpdate<TubeWorm.Tongue>(tongue => tongue.worm.Grabber);
}
