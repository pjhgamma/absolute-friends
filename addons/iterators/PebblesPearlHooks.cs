using MoreSlugcats;
using RippleFriends.Hooks;

namespace RippleFriends.Iterators;

internal class PebblesPearlHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.PebblesPearl];

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        return obj is not HalcyonPearl && orig(self, obj);
    }
}
