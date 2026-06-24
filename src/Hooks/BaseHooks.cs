using System.Reflection;

namespace RippleFriends.Hooks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class HookPatchAttribute(Type targetType, string eventName) : Attribute
{
    public Type TargetType { get; } = targetType;

    public string EventName { get; } = eventName;
}

internal abstract class BaseHooks
{
    /// <summary>
    /// The option that turns this group of hooks on. The option itself rather than its value, so that code
    /// outside can reach the option a group of hooks belongs to, not just whether it is currently on.
    /// </summary>
    protected abstract Configurable<bool> Option { get; }

    public virtual bool IsEnabled => Option.Value;

    private bool _isInit;

    private string? _name;

    private readonly List<Action> _hookActions = [];

    private readonly List<Action> _unhookActions = [];

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

            _name = type.Name;
            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (methodInfo?.GetCustomAttribute<HookPatchAttribute>() is { } attr)
                {
                    try
                    {
                        if (attr.TargetType?.GetEvent(attr.EventName, BindingFlags.Static | BindingFlags.Public) is { } eventInfo)
                        {
                            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, methodInfo.IsStatic ? null : this, methodInfo);

                            _hookActions.Add(() => eventInfo.AddEventHandler(null, handler));
                            _unhookActions.Add(() => eventInfo.RemoveEventHandler(null, handler));
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

            UnityEngine.Debug.Log($"Ripple Friends: {_name}: Hooks failed: {exception.Message}");
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

        try
        {
            foreach (var action in _hookActions)
            {
                action?.Invoke();
            }

            UnityEngine.Debug.Log($"Ripple Friends: {_name}: Hooks applied");
        }
        catch (Exception exception)
        {
            Disable();

            UnityEngine.Debug.Log($"Ripple Friends: {_name}: Hooks failed: {exception}");
        }
    }

    public void Disable()
    {
        try
        {
            foreach (var action in _unhookActions)
            {
                action?.Invoke();
            }
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.Log($"Ripple Friends: {_name}: Hooks failed: {exception}");
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
