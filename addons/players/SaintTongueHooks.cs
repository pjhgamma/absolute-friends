using MonoMod.Cil;
using RippleFriends.Core;
using RippleFriends.Hooks;

namespace RippleFriends.Players;

internal class SaintTongueHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.SaintTongue];

    [HookPatch(typeof(IL.Player.Tongue), nameof(IL.Player.Tongue.Update))]
    [HookTest([195], ["ldfld CollisionResult::chunk"])]
    private static void IL_Player_Tongue_Update(ILContext il) => il.TongueUpdate<Player.Tongue>(tongue => tongue?.player);
}
