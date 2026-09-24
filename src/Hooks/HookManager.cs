using AbsoluteFriends.Addons;
using AbsoluteFriends.Diagnostics;
using System.Reflection;

namespace AbsoluteFriends.Hooks;

internal static class HookManager
{
    private static readonly List<BaseHooks> _activeHooks = [];

    private static readonly HashSet<Assembly> _assemblies = [];

    private static readonly HashSet<OptionInterface> _menus = [];

    private static bool _isInit;

    private static bool _isBinding;

    internal static IEnumerable<HookBinding> Bindings => _activeHooks.SelectMany(hook => hook.Bindings);

    internal static void Register(Assembly assembly)
    {
        if (!_assemblies.Add(assembly))
        {
            return;
        }

        try
        {
            IEnumerable<Type> hookTypes = assembly.GetTypes().Where(
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
            Reporter.LogError($"{assembly.GetName().Name}: Hooks could not be collected", exception);
        }

        if (_isInit && !_isBinding)
        {
            OnEnable();
        }
    }

    internal static void Unregister(Assembly assembly)
    {
        if (!_assemblies.Remove(assembly))
        {
            return;
        }

        foreach (var hook in _activeHooks.Where(hook => hook.GetType().Assembly == assembly).ToArray())
        {
            hook.Disable();

            _activeHooks.Remove(hook);
        }
    }

    internal static void Watch(OptionInterface menu) => _menus.Add(menu);

    internal static bool IsFailed(Configurable<bool>? option) => option != null && _activeHooks.Any(hook => hook.HasFailed && hook.Owns(option));

    internal static bool IsWarned(Configurable<bool>? option) => option != null && _activeHooks.Any(hook => hook.HasWarned && hook.Owns(option));

    internal static void Initialize()
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

            Watch(Options.RemixMenu.Instance);
            Register(Assembly.GetExecutingAssembly());
        }
        catch (Exception exception)
        {
            Reporter.LogError("Hook Manager initialization failed", exception);
        }
    }

    internal static void OnEnable()
    {
        if (_isBinding)
        {
            return;
        }

        _isBinding = true;

        try
        {
            Reporter.BeginSession();

            Initialize();

            AddonRegistry.UpdateRunningStates();

            foreach (var hook in _activeHooks)
            {
                hook.ClearWarnings();
            }

            foreach (var hook in _activeHooks.ToArray())
            {
                hook.Enable();
            }

            foreach (var binding in Bindings)
            {
                binding.Report();
            }

            Reporter.EndSession();
        }
        finally
        {
            _isBinding = false;
        }
    }

    internal static void OnDisable()
    {
        _isInit = false;

        On.OptionInterface._SaveConfigFile -= On_OptionInterface__SaveConfigFile;

        HookNotifier.Stop();

        AddonRegistry.Reset();

        foreach (var hooks in _activeHooks)
        {
            hooks.Disable();
        }

        _activeHooks.Clear();
        _assemblies.Clear();
        _menus.Clear();
    }

    internal static void On_OptionInterface__SaveConfigFile(On.OptionInterface.orig__SaveConfigFile orig, OptionInterface self)
    {
        orig(self);

        if (!_menus.Contains(self))
        {
            return;
        }

        OnEnable();

        Reporter.LogInfo("Configurations saved");
    }
}
