using System.Reflection;

namespace RippleFriends.Hooks;

internal static class HookManager
{
    private static bool _isInit;

    private static readonly List<BaseHooks> _activeHooks = [];

    public static void Initialize()
    {
        if (_isInit)
        {
            return;
        }

        _isInit = true;

        try
        {
            On.OptionInterface._SaveConfigFile -= On_OptionInterface__SaveConfigFile;
            On.OptionInterface._SaveConfigFile += On_OptionInterface__SaveConfigFile;

            var hookTypes = Assembly.GetExecutingAssembly().GetTypes().Where(
                t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseHooks))
            );

            foreach (var type in hookTypes)
            {
                if (Activator.CreateInstance(type) is BaseHooks hook)
                {
                    _activeHooks.Add(hook);
                }
            }
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.Log($"Ripple Friends: Hook Manager initialization failed: {exception}");
        }
    }

    public static void OnEnable()
    {
        Initialize();

        foreach (var hook in _activeHooks)
        {
            hook.Enable();
        }
    }

    public static void OnDisable()
    {
        if (_activeHooks == null)
        {
            return;
        }

        foreach (var hooks in _activeHooks)
        {
            hooks.Disable();
        }
        _activeHooks.Clear();
    }

    public static void On_OptionInterface__SaveConfigFile(On.OptionInterface.orig__SaveConfigFile orig, OptionInterface self)
    {
        orig(self);

        OnEnable();

        UnityEngine.Debug.Log($"Ripple Friends: Configurations saved");
    }
}
