using RippleFriends.Core;
using RippleFriends.Hooks;
using RippleFriends.Utils;

namespace RippleFriends.Players;

internal class GrabbingPlayerHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.GrabbingPlayer];

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        return !(obj is Player player && obj.IsPlayer && !player.dead && self.IsFriend(player) && !player.IsIdlePlayer(Config.GrabbingPlayerTime.Value)) && orig(self, obj);
    }
}
