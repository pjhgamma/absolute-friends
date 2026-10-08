using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Players;

internal class WiggleHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Wiggle];

    [HookPatch(typeof(On.Player), nameof(On.Player.Update))]
    private static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.IsPlayer)
        {
            return;
        }

        for (int i = self.grabbedBy.Count - 1; i >= 0; --i)
        {
            if (i >= self.grabbedBy.Count)
            {
                continue;
            }

            if (self.GraspWiggle > UnityEngine.Random.value && self.grabbedBy[i] is { grabber: Player grabber } grasp && self.IsFriend(grabber))
            {
                grasp.Release();
            }
        }
    }
}
