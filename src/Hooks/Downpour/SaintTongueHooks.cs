using MonoMod.Cil;
using static RippleFriends.Core.ILUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Downpour;

internal class SaintTongueHooks : DownpourHooks
{
    protected override Configurable<bool> Option => Config.SaintTongue;

    [HookPatch(typeof(IL.Player.Tongue), nameof(IL.Player.Tongue.Update))]
    [HookTest([195], ["ldfld CollisionResult::chunk"])]
    private static void IL_Player_Tongue_Update(ILContext il)
    {
        IL_Tongue<Player.Tongue>(
            il,
            tongue => tongue.player
        );
    }
}
