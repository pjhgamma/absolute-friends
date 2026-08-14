using MonoMod.Cil;
using RippleFriends.Diagnostics;
using System.Reflection;

namespace RippleFriends.Hooks;

internal sealed class HookBinding(BaseHooks owner, string methodName, EventInfo eventInfo, Delegate handler, HookTestAttribute test)
{
    public readonly string MethodName = methodName;

    public string HookName => owner.Name;

    public readonly Delegate Handler = handler;

    public int[] Indices => test.Indices;

    public string[] Anchors => test.Anchors;

    public bool Failed;

    public bool Warned;

    public bool Ran;

    public string FullName => $"{HookName}.{MethodName}";

    private Delegate? _applied;

    private Delegate? _guard;

    public void Apply()
    {
        Remove();

        Failed = false;
        Warned = false;
        Ran = false;
        _guard ??= HookGuard.Wrap(this, eventInfo.EventHandlerType);
        _applied = _guard ?? Handler;

        eventInfo.AddEventHandler(null, _applied);
    }

    public void Remove()
    {
        if (_applied == null)
        {
            return;
        }

        Delegate applied = _applied;

        _applied = null;
        eventInfo.RemoveEventHandler(null, applied);
    }

    public void MarkWarned() => Warned = true;

    public void HandleError(Exception exception)
    {
        owner.MarkFailed();

        HookDiagnostics.LogError($"{FullName} ({eventInfo.DeclaringType?.FullName}.{eventInfo.Name}) threw {exception.GetType().Name}", exception);

        HookNotifier.Queue(owner.Title);
    }
}
