using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Players;

internal class MaulHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Mauling];

    [HookPatch(typeof(On.Player), nameof(On.Player.CanEatMeat))]
    private static bool On_Player_CanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
    {
        return !crit.IsFriend(self) && orig(self, crit);
    }
}

internal class DownpourMaulHooks : DownpourHooks
{
    protected override Configurable<bool>[] Options => [Config.Mauling];

    [HookPatch(typeof(On.Player), nameof(On.Player.CanMaulCreature))]
    private static bool On_Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
    {
        return !crit.IsFriend(self) && orig(self, crit);
    }
}
