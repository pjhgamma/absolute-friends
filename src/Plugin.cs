using BepInEx;
using BepInEx.Logging;
using RippleFriends.Hooks;
using RippleFriends.Options;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace RippleFriends;

[BepInPlugin(GUID, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "pjhgamma.ripplefriends";

    public const string Name = "Ripple Friends";

    public const string Version = "0.1.0";

    public bool _isInit;

    private void OnEnable()
    {
        On.RainWorld.OnModsInit += On_RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += On_RainWorld_PostModsInit;
    }

    private void OnDisable()
    {
        if (!_isInit)
        {
            return;
        }

        _isInit = false;

        On.RainWorld.OnModsInit -= On_RainWorld_OnModsInit;
        On.RainWorld.PostModsInit -= On_RainWorld_PostModsInit;

        HookManager.OnDisable();
    }

    private void On_RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;

            MachineConnector.SetRegisteredOI(GUID, RemixMenu.Instance);

            UnityEngine.Debug.Log($"Ripple Friends: Plugin initialized");
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.Log($"Ripple Friends: Plugin initialization failed: {exception.Message}");
        }
    }

    private void On_RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        HookManager.OnEnable();
    }
}
