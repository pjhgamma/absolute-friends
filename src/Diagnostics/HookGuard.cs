using MonoMod.Cil;
using RippleFriends.Hooks;

namespace RippleFriends.Diagnostics;

internal static class HookGuard
{
    private static readonly GuardContract _bindingContract = new(
        typeof(HookBinding),
        nameof(HookBinding.Failed),
        nameof(HookBinding.Handler),
        nameof(HookBinding.HandleError),
        nameof(HookBinding.Ran)
    );

    private static HookBinding? Manipulating { get; set; }

    public static Delegate? Wrap(HookBinding binding, Type handlerType)
    {
        try
        {
            return handlerType == typeof(ILContext.Manipulator) ? CreateManipulatorGuard(binding) : GuardEmitter.Wrap(handlerType, binding, _bindingContract, $"RippleFriends_Guard_{binding.HookName}_{binding.MethodName}");
        }
        catch (Exception exception)
        {
            HookDiagnostics.LogWarning($"{binding.FullName}: could not be guarded, binding it directly instead", exception);

            binding.MarkWarned();

            return null;
        }
    }

    public static void EmitGuarded<T1, T2>(this ILCursor c, Action<T1, T2> body)
    {
        HookBinding? binding = Manipulating;

        c.EmitDelegate((T1 first, T2 second) =>
        {
            if (!ShouldRun(binding))
            {
                return;
            }

            try
            {
                body(first, second);
            }
            catch (Exception exception)
            {
                Report(binding, exception);
            }
        });
    }

    public static void EmitGuarded<T1, TResult>(this ILCursor c, Func<T1, TResult> body, Func<T1, TResult>? fallback = null)
    {
        HookBinding? binding = Manipulating;

        c.EmitDelegate((T1 first) =>
        {
            if (!ShouldRun(binding))
            {
                return Passthrough(fallback, first);
            }

            try
            {
                return body(first);
            }
            catch (Exception exception)
            {
                Report(binding, exception);

                return Passthrough(fallback, first);
            }
        });
    }

    public static void EmitGuarded<T1, T2, TResult>(this ILCursor c, Func<T1, T2, TResult> body, Func<T1, T2, TResult>? fallback = null)
    {
        HookBinding? binding = Manipulating;

        c.EmitDelegate((T1 first, T2 second) =>
        {
            if (!ShouldRun(binding))
            {
                return Passthrough(fallback, first, second);
            }

            try
            {
                return body(first, second);
            }
            catch (Exception exception)
            {
                Report(binding, exception);

                return Passthrough(fallback, first, second);
            }
        });
    }

    private static ILContext.Manipulator CreateManipulatorGuard(HookBinding binding)
    {
        ILContext.Manipulator manipulator = (ILContext.Manipulator)binding.Handler;

        return il =>
        {
            ILSnapshot snapshot = ILVerifier.Capture(il);

            Manipulating = binding;

            try
            {
                manipulator(il);
            }
            catch (Exception exception)
            {
                binding.HandleError(exception);

                throw;
            }
            finally
            {
                Manipulating = null;
            }

            try
            {
                if (!ILVerifier.Verify(binding.FullName, snapshot, il, binding.Indices, binding.Anchors))
                {
                    binding.MarkWarned();
                }
            }
            catch (Exception exception)
            {
                HookDiagnostics.LogWarning($"{binding.FullName}: verification itself failed", exception);
            }
        };
    }

    private static TResult Passthrough<T1, TResult>(Func<T1, TResult>? fallback, T1 first) => fallback == null ? default! : fallback(first);

    private static TResult Passthrough<T1, T2, TResult>(Func<T1, T2, TResult>? fallback, T1 first, T2 second) => fallback == null ? default! : fallback(first, second);

    private static bool ShouldRun(HookBinding? binding)
    {
        if (binding == null)
        {
            return true;
        }

        binding.Ran = true;

        return !binding.Failed;
    }

    private static void Report(HookBinding? binding, Exception exception)
    {
        if (binding == null)
        {
            HookDiagnostics.LogError($"Emitted code threw {exception.GetType().Name} and was disabled", exception);

            return;
        }

        binding.HandleError(exception);
    }
}
