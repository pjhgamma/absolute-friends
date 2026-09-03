using RippleFriends.Hooks;
using RippleFriends.Options;

namespace RippleFriends.Gameplay.Iterators;

internal class MoonNeuronHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.MoonNeuron];

    [HookPatch(typeof(On.Player), nameof(On.Player.CanIPickThisUp))]
    private static bool On_Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        return obj is not SLOracleSwarmer && orig(self, obj);
    }
}
