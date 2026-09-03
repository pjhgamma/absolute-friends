using RippleFriends.Diagnostics;
using RippleFriends.Options;
using System.Reflection;

namespace RippleFriends.Hooks;

internal static class HookManager
{
    private static readonly List<BaseHooks> _activeHooks = [];

    private static bool _isInit;

    public static IEnumerable<HookBinding> Bindings => _activeHooks.SelectMany(hook => hook.Bindings);

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

            HookNotifier.Start();

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
            hook.ClearWarnings();
        }

        foreach (var hook in _activeHooks)
        {
            hook.Enable();
        }

        foreach (var binding in Bindings)
        {
            binding.Report();
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

    public static bool IsFailed(Configurable<bool>? option) => option != null && _activeHooks.Any(hook => hook.HasFailed && hook.Owns(option));

    public static bool IsWarned(Configurable<bool>? option) => option != null && _activeHooks.Any(hook => hook.HasWarned && hook.Owns(option));

    public static void On_OptionInterface__SaveConfigFile(On.OptionInterface.orig__SaveConfigFile orig, OptionInterface self)
    {
        orig(self);

        if (!ReferenceEquals(self, RemixMenu.Instance))
        {
            return;
        }

        OnEnable();

        HookDiagnostics.LogInfo("Configurations saved");
    }
}
