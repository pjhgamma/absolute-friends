using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Options;
using System.Reflection;

namespace AbsoluteFriends.Hooks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class HookPatchAttribute(Type targetType, string eventName) : Attribute
{
    public Type TargetType { get; } = targetType;

    public string EventName { get; } = eventName;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class HookTestAttribute(int[] indices, string[] anchors) : Attribute
{
    public int[] Indices { get; } = indices;

    public string[] Anchors { get; } = anchors;
}

public abstract class BaseHooks
{
    private readonly List<HookBinding> _bindings = [];

    private bool _isInit;

    public bool HasFailed { get; private set; }

    public bool HasWarned { get; private set; }

    public virtual bool IsEnabled => IsOptionEnabled && !HasFailed;

    public string Name => field ??= GetType().Name;

    public string Title => Subject ?? Options.FirstOrDefault().Label ?? Name;

    internal IEnumerable<HookBinding> Bindings => _bindings;

    protected abstract Configurable<bool>[] Options { get; }

    protected virtual string? Subject => null;

    protected virtual bool IsOptionEnabled => Options.Any(option => option.IsActive);

    internal void MarkWarned() => HasWarned = true;

    internal void ClearWarnings()
    {
        HasWarned = false;

        foreach (var binding in _bindings)
        {
            binding.Warned = false;

            binding.ClearReport();
        }
    }

    internal void MarkFailed()
    {
        HasFailed = true;

        foreach (var binding in _bindings)
        {
            binding.Failed = true;
        }
    }

    internal bool Owns(Configurable<bool>? option) => option != null && Options.Any(owned => ReferenceEquals(owned, option));

    internal void Enable()
    {
        Initialize();
        Disable();

        if (!IsEnabled)
        {
            return;
        }

        int isApplied = 0;

        foreach (var binding in _bindings)
        {
            try
            {
                binding.Apply();

                isApplied++;
            }
            catch (Exception exception)
            {
                try
                {
                    binding.Remove();
                }
                catch
                {
                }

                MarkFailed();

                Reporter.LogError($"{binding.FullName} could not be applied", exception);
                HookNotifier.Queue(Title);

                break;
            }
        }

        Reporter.LogInfo($"{Name}: Hooks applied ({isApplied}/{_bindings.Count})");
    }

    internal void Disable()
    {
        foreach (var binding in _bindings)
        {
            try
            {
                binding.Remove();
            }
            catch (Exception exception)
            {
                Reporter.LogWarning($"{binding.FullName}: Unhook failed", exception);
            }
        }
    }

    private void Initialize()
    {
        if (_isInit)
        {
            return;
        }

        _isInit = true;

        try
        {
            Type type = GetType();

            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (methodInfo?.GetCustomAttribute<HookPatchAttribute>() is { } attr)
                {
                    try
                    {
                        if (attr.TargetType?.GetEvent(attr.EventName, BindingFlags.Static | BindingFlags.Public) is { } eventInfo)
                        {
                            Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, methodInfo.IsStatic ? null : this, methodInfo);

                            _bindings.Add(new(this, methodInfo.Name, eventInfo, handler, methodInfo.GetCustomAttribute<HookTestAttribute>()));
                        }
                        else
                        {
                            throw new($"Failed to bind event {attr.EventName} on {attr.TargetType?.Name}");
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new($"{attr.EventName} failed", exception);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Disable();

            Reporter.LogError($"{Name}: Hooks failed", exception);
        }
    }
}

public abstract class DownpourHooks : BaseHooks
{
    public override bool IsEnabled => base.IsEnabled && ModManager.MSC;
}

public abstract class WatcherHooks : BaseHooks
{
    public override bool IsEnabled => base.IsEnabled && ModManager.Watcher;
}
