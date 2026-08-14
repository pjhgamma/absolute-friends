using RippleFriends.Diagnostics;
using System.Reflection;

namespace RippleFriends.Hooks;

internal static class HookManager
{
    public static IEnumerable<HookBinding> Bindings => _activeHooks.SelectMany(hook => hook.Bindings);

    private static bool _isInit;

    private static readonly List<BaseHooks> _activeHooks = [];

    public static bool IsFailed(Configurable<bool>? option) => option != null && _activeHooks.Any(hook => hook.HasFailed && hook.Owns(option));

    public static bool IsWarned(Configurable<bool>? option) => option != null && _activeHooks.Any(hook => hook.HasWarned && hook.Owns(option));

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

            HookNotifier.Listen();

            IEnumerable<Type> hookTypes = Assembly.GetExecutingAssembly().GetTypes().Where(
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
            HookDiagnostics.LogError("Hook Manager initialization failed", exception);
        }
    }

    public static void OnEnable()
    {
        HookDiagnostics.BeginSession();

        Initialize();

        foreach (var hook in _activeHooks)
        {
            hook.Enable();
        }

        HookDiagnostics.EndSession();
    }

    public static void OnDisable()
    {
        _isInit = false;

        On.OptionInterface._SaveConfigFile -= On_OptionInterface__SaveConfigFile;

        HookNotifier.Stop();

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

        HookDiagnostics.LogInfo("Configurations saved");
    }
}
