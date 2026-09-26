using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Players;

internal class GrabbingPlayerHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.GrabbingPlayer];

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        if (
            obj is Player player
            && obj.IsPlayer
            && player.Consious
            && player.touchedNoInputCounter <= Config.GrabbingPlayerTime.Value * RainWorldUtils.Second
            && self.IsFriend(player)
        )
        {
            return false;
        }

        return orig(self, obj);
    }
}
