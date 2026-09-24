using AbsoluteFriends.Core;
using AbsoluteFriends.Hooks;

namespace AbsoluteFriends.Creatures;

internal class ScavengerTemplarHooks : WatcherHooks
{
    protected override Configurable<bool>[] Options => [Config.ScavengerTemplar];

    [HookPatch(typeof(On.KarmicShockwave), nameof(On.KarmicShockwave.StunCreatures))]
    private static void On_KarmicShockwave_StunCreatures(On.KarmicShockwave.orig_StunCreatures orig, KarmicShockwave self)
    {
        foreach (var creature in self.room.FriendsOf(self.source))
        {
            self.creaturesHit.Add(creature);
        }

        orig(self);
    }
}
