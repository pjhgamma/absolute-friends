using AbsoluteFriends.Hooks;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.Diagnostics;

internal class ReportHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Debug];

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ctor))]
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        Reporter.SaveReport();
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ShutDownProcess))]
    private static void On_RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);

        Reporter.SaveReport();
    }
}
