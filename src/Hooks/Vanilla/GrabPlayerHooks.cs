using static RippleFriends.Core.PlayerUtils;
using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class GrabPlayerHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.GrabPlayer;

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        if (obj is Player player && !player.dead && !IsIdlePlayer(player, Config.GrabPlayerTime.Value))
        {
            return false;
        }
        return orig(self, obj);
    }
}
