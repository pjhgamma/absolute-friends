using System.Runtime.CompilerServices;
using AbsoluteFriends.Hooks;
using JollyCoop.JollyHUD;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal class PlayerIconHooks : BaseHooks
{
    private static readonly ConditionalWeakTable<JollyMeter.PlayerIcon, StrongBox<float>> _intensities = new();

    protected override Configurable<bool>[] Options => [Config.FriendMeter];

    [HookPatch(typeof(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon), nameof(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.Update))]
    private static void On_PlayerIcon_Update(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.orig_Update orig, JollyMeter.PlayerIcon self)
    {
        int blinkRed = self.blinkRed;

        self.blinkRed = 0;
        orig(self);
        self.blinkRed = Math.Max(0, blinkRed - 1);

        StrongBox<float> intensity = _intensities.GetOrCreateValue(self);

        intensity.Value = self.dead ? 0f : Mathf.MoveTowards(intensity.Value, blinkRed > 0 ? 1f : 0f, OverlayUtils.GatePulseStep);

        if (intensity.Value <= 0f)
        {
            return;
        }

        RainWorldGame? game = self.player?.world?.game;
        float scale = self.rad * OverlayUtils.GatePulseScale(game, 0f, intensity.Value);

        self.iconSprite.scale = scale;
        self.gradient.scale = self.baseGradScale * scale;
        self.color = OverlayUtils.GatePulseColor(self.color, game, 0f, intensity.Value);
    }
}
