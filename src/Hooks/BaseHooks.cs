using RippleFriends.Diagnostics;
using RippleFriends.Options;
using System.Reflection;

namespace RippleFriends.Hooks;

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

internal abstract class BaseHooks
{
    protected abstract Configurable<bool> Option { get; }

    public bool HasFailed { get; private set; }

    public bool HasWarned => _bindings.Any(binding => binding.Warned);

    public virtual bool IsEnabled => Option.Value && !HasFailed;

    public string Name => field ??= GetType().Name;

    public string Title => Option.Label() ?? Name;

    public IEnumerable<HookBinding> Bindings => _bindings;

    private bool _isInit;

    private readonly List<HookBinding> _bindings = [];

    public void MarkFailed()
    {
        HasFailed = true;

        foreach (var binding in _bindings)
        {
            binding.Failed = true;
        }
    }

    public bool Owns(Configurable<bool>? option) => option != null && ReferenceEquals(Option, option);

    private void Initialize()
    {
        if (_isInit)
        {
            return;
        }

        _isInit = true;

        try
        {
            var type = GetType();

            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (methodInfo?.GetCustomAttribute<HookPatchAttribute>() is { } attr)
                {
                    try
                    {
                        if (attr.TargetType?.GetEvent(attr.EventName, BindingFlags.Static | BindingFlags.Public) is { } eventInfo)
                        {
                            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, methodInfo.IsStatic ? null : this, methodInfo);

                            _bindings.Add(new(this, methodInfo.Name, eventInfo, handler, methodInfo.GetCustomAttribute<HookTestAttribute>()));
                        }
                        else
                        {
                            throw new($"Failed to bind event {attr.EventName} on {attr.TargetType?.Name}");
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new($"{attr.EventName} failed: {exception}");
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Disable();

            HookDiagnostics.LogError($"{Name}: Hooks failed", exception);
        }
    }

    public void Enable()
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

                HookDiagnostics.LogError($"{binding.FullName} could not be applied", exception);
                HookNotifier.Queue(Title);

                break;
            }
        }

        HookDiagnostics.LogInfo($"{Name}: Hooks applied ({isApplied}/{_bindings.Count})");
    }

    public void Disable()
    {
        foreach (var binding in _bindings)
        {
            try
            {
                binding.Remove();
            }
            catch (Exception exception)
            {
                HookDiagnostics.LogWarning($"{binding.FullName}: Unhook failed", exception);
            }
        }
    }
}

internal abstract class DownpourHooks : BaseHooks
{
    public override bool IsEnabled => base.IsEnabled && ModManager.MSC;
}

internal abstract class WatcherHooks : BaseHooks
{
    public override bool IsEnabled => base.IsEnabled && ModManager.Watcher;
}
