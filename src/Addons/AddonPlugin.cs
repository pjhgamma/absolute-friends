using BepInEx;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.Addons;

public abstract class AddonPlugin : BaseUnityPlugin
{
    private bool _isInit;

    public virtual string AddonDescription => "";

    public abstract string AbsoluteFriendsVersion { get; }

    public virtual string[] AddonDependencies => [];

    protected Addon AddonContext { get; private set; } = null!;

    protected new AddonLogger Logger => Reporter.GetLogger(Info.Metadata.Name);

    protected virtual OptionInterface? RemixMenu => null;

    internal void Activate()
    {
        try
        {
            Enable();

            Logger.LogInfo("Enabled");
        }
        catch (Exception exception)
        {
            Logger.LogError("Could not enable", exception);
        }
    }

    internal void Deactivate()
    {
        try
        {
            Disable();

            Logger.LogInfo("Disabled");
        }
        catch (Exception exception)
        {
            Logger.LogError("Could not disable", exception);
        }
    }

    protected virtual void Bind(Addon addon)
    {
    }

    protected virtual void BuildMenu(MenuBuilder menu)
    {
    }

    protected virtual void Enable()
    {
    }

    protected virtual void Disable()
    {
    }

    private void OnEnable()
    {
        On.RainWorld.OnModsInit -= On_RainWorld_OnModsInit;
        On.RainWorld.OnModsInit += On_RainWorld_OnModsInit;
    }

    private void OnDisable()
    {
        On.RainWorld.OnModsInit -= On_RainWorld_OnModsInit;

        if (!_isInit)
        {
            return;
        }

        _isInit = false;

        AddonRegistry.Remove(this);
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

            if (RemixMenu is { } pluginMenu)
            {
                MachineConnector.SetRegisteredOI(Info.Metadata.GUID, pluginMenu);
            }

            AddonContext = AddonRegistry.Add(this, BuildMenu);

            Bind(AddonContext);

            Logger.LogInfo("Registered");

            if (AddonContext.IsMismatched)
            {
                Logger.LogWarning($"Built for Absolute Friends v{AddonContext.AbsoluteFriendsVersion}, running v{Plugin.Version}");
            }
        }
        catch (Exception exception)
        {
            Logger.LogError("Could not register", exception);
        }
    }
}
