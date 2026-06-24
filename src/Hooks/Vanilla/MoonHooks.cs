using RippleFriends.Options;

namespace RippleFriends.Hooks.Vanilla;

internal class MoonHooks : BaseHooks
{
    protected override Configurable<bool> Option => Config.Moon;

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        if (obj is SLOracleSwarmer)
        {
            return false;
        }
        return orig(self, obj);
    }
}
