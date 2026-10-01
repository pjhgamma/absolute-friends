using AbsoluteFriends.Hooks;
using Menu;
using UnityEngine;

namespace AbsoluteFriends.Core.Manager;

internal class FriendManagerHooks : BaseHooks
{
    private static float _shortcutTime = float.NegativeInfinity;

    private static bool _shortcutPending;

    private static int _shortcutFrame = -1;

    protected override Configurable<bool>[] Options => [];

    protected override bool IsOptionEnabled => true;

    protected override string? Subject => "Friend Manager Shortcut";

    internal static bool ConsumeShortcut()
    {
        if (!_shortcutPending || Time.unscaledTime - _shortcutTime > 0.2f)
        {
            _shortcutPending = false;

            return false;
        }

        _shortcutPending = false;

        return true;
    }

    private static void TryOpen(ProcessManager manager, RainWorldGame game)
    {
        if (manager.IsSwitchingProcesses())
        {
            _shortcutPending = false;

            return;
        }

        if (manager.IsRunningAnyDialog || !ConsumeShortcut())
        {
            return;
        }

        FriendManagerDialog dialog = new(manager, game);

        manager.ShowDialog(dialog);

        if (ReferenceEquals(manager.dialog, dialog))
        {
            dialog.PauseGame();
        }
    }

    [HookPatch(typeof(On.RainWorld), nameof(On.RainWorld.Update))]
    private static void On_RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        KeyCode key = Config.FriendManagerKey.Value;

        if (key != KeyCode.None && Time.frameCount != _shortcutFrame && Input.GetKeyDown(key))
        {
            _shortcutFrame = Time.frameCount;
            _shortcutTime = Time.unscaledTime;
            _shortcutPending = true;
        }

        orig(self);
    }

    [HookPatch(typeof(On.SaveState), nameof(On.SaveState.LoadGame))]
    private static void On_SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string saveString, RainWorldGame game)
    {
        orig(self, saveString, game);

        FriendUtils.ResetTrackingChoices();
        FriendDisplayNames.ResetNicknames();
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ctor))]
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        FriendUtils.ResetTrackingChoices();
        FriendDisplayNames.ResetNicknames();
    }

    [HookPatch(typeof(On.Menu.PauseMenu), nameof(On.Menu.PauseMenu.Update))]
    private static void On_PauseMenu_Update(On.Menu.PauseMenu.orig_Update orig, PauseMenu self)
    {
        if (self.manager.dialog is FriendManagerDialog)
        {
            return;
        }

        orig(self);

        TryOpen(self.manager, self.game);
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.RawUpdate))]
    private static void On_RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
    {
        if (self.manager.dialog is not FriendManagerDialog)
        {
            orig(self, dt);

            return;
        }

        bool devToolsActive = self.devToolsActive;

        try
        {
            self.devToolsActive = false;
            self.oDown = true;

            orig(self, dt);
        }
        finally
        {
            self.devToolsActive = devToolsActive;
        }
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.Update))]
    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        TryOpen(self.manager, self);

        orig(self);
    }
}
