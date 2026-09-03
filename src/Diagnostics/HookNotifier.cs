using RippleFriends.Utils;

namespace RippleFriends.Diagnostics;

internal static class HookNotifier
{
    private const int MessageDelay = 0;

    private const int MessageDuration = 200;

    private static readonly List<string> _pending = [];

    private static readonly HashSet<string> _announced = [];

    public static void Start()
    {
        On.RainWorldGame.Update -= On_RainWorldGame_Update;
        On.RainWorldGame.Update += On_RainWorldGame_Update;
    }

    public static void Stop()
    {
        On.RainWorldGame.Update -= On_RainWorldGame_Update;

        _pending.Clear();
    }

    public static void Queue(string feature)
    {
        if (_announced.Add(feature))
        {
            _pending.Add(feature);
        }
    }

    private static void On_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        if (_pending.Count == 0)
        {
            return;
        }

        try
        {
            Deliver(self);
        }
        catch (Exception exception)
        {
            _pending.Clear();

            HookDiagnostics.LogWarning("Could not show the text prompt", exception);
        }
    }

    private static void Deliver(RainWorldGame game)
    {
        if (RainWorldUtils.MainCamera(game) is not { } camera || camera.hud?.textPrompt is not { } textPrompt)
        {
            return;
        }

        string features = string.Join(", ", _pending.Select(Translation.Of));

        _pending.Clear();

        textPrompt.AddMessage(
            Translation.Of("Ripple Friends: an error turned off <PLACEHOLDER>. See the Remix menu.").Replace(Translation.Placeholder, features),
            MessageDelay,
            MessageDuration,
            false,
            false
        );

        camera.virtualMicrophone?.PlaySound(SoundID.MENU_Error_Ping, 0f, 1f, 1f);
    }
}
