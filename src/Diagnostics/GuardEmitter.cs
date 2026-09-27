using System.Reflection.Emit;
using System.Reflection;

namespace AbsoluteFriends.Diagnostics;

internal static class GuardContexts
{
    public static object[] Slots = [];

    private static readonly object _lock = new();

    public static int Register(object context)
    {
        lock (_lock)
        {
            object[] slots = Slots;
            object[] grown = new object[slots.Length + 1];

            Array.Copy(slots, grown, slots.Length);

            grown[slots.Length] = context;
            Slots = grown;

            return slots.Length;
        }
    }
}

internal sealed class GuardContract
{
    public readonly Type ContextType;

    public readonly FieldInfo FailedField;

    public readonly FieldInfo HandlerField;

    public readonly MethodInfo HandleErrorMethod;

    public readonly FieldInfo RanField;

    public GuardContract(Type contextType, string failedFieldName, string handlerFieldName, string handleErrorMethodName, string ranFieldName)
    {
        ContextType = contextType;
        FailedField = contextType.GetField(failedFieldName) ?? throw new ArgumentException($"{contextType.Name} has no public field {failedFieldName}");
        HandlerField = contextType.GetField(handlerFieldName) ?? throw new ArgumentException($"{contextType.Name} has no public field {handlerFieldName}");
        HandleErrorMethod = contextType.GetMethod(handleErrorMethodName) ?? throw new ArgumentException($"{contextType.Name} has no public method {handleErrorMethodName}");
        RanField = contextType.GetField(ranFieldName) ?? throw new ArgumentException($"{contextType.Name} has no public field {ranFieldName}");

        if (RanField.FieldType != typeof(bool))
        {
            throw new ArgumentException($"{ranFieldName} must be a bool");
        }

        if (FailedField.FieldType != typeof(bool))
        {
            throw new ArgumentException($"{failedFieldName} must be a bool");
        }

        if (!typeof(Delegate).IsAssignableFrom(HandlerField.FieldType))
        {
            throw new ArgumentException($"{handlerFieldName} must be a delegate");
        }

        if (HandleErrorMethod.ReturnType != typeof(bool))
        {
            throw new ArgumentException($"{handleErrorMethodName} must return whether the hook is to blame");
        }
    }
}

internal interface IEmittedGuard
{
    Delegate Body { get; }

    Delegate? Fallback { get; }

    bool ShouldRun();

    void MarkFailed(Exception exception);
}

internal static class GuardEmitter
{
    private static readonly FieldInfo _slotsField = typeof(GuardContexts).GetField(nameof(GuardContexts.Slots));

    private static readonly MethodInfo _bodyGetter = typeof(IEmittedGuard).GetProperty(nameof(IEmittedGuard.Body)).GetGetMethod();

    private static readonly MethodInfo _fallbackGetter = typeof(IEmittedGuard).GetProperty(nameof(IEmittedGuard.Fallback)).GetGetMethod();

    private static readonly MethodInfo _shouldRunMethod = typeof(IEmittedGuard).GetMethod(nameof(IEmittedGuard.ShouldRun));

    private static readonly MethodInfo _markFailedMethod = typeof(IEmittedGuard).GetMethod(nameof(IEmittedGuard.MarkFailed));

    public static Delegate? Wrap(Type handlerType, object context, GuardContract contract, string methodName)
    {
        if (handlerType.GetMethod("Invoke") is not { } invoke)
        {
            return null;
        }

        ParameterInfo[] parameters = invoke.GetParameters();
        Type returnType = invoke.ReturnType;
        MethodInfo? origInvoke = FindOriginalInvoke(parameters, returnType);

        Type[] argumentTypes = [.. parameters.Select(parameter => parameter.ParameterType)];

        DynamicMethod method = new(methodName, returnType, argumentTypes, contract.ContextType, true);
        ILGenerator il = method.GetILGenerator();

        LocalBuilder? result = returnType == typeof(void) ? null : il.DeclareLocal(returnType);
        LocalBuilder error = il.DeclareLocal(typeof(Exception));
        LocalBuilder contextLocal = il.DeclareLocal(contract.ContextType);
        Label fallback = il.DefineLabel();
        Label done = il.DefineLabel();

        il.Emit(OpCodes.Ldsfld, _slotsField);
        il.Emit(OpCodes.Ldc_I4, GuardContexts.Register(context));
        il.Emit(OpCodes.Ldelem_Ref);
        il.Emit(OpCodes.Castclass, contract.ContextType);
        il.Emit(OpCodes.Stloc, contextLocal);

        il.Emit(OpCodes.Ldloc, contextLocal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stfld, contract.RanField);

        il.Emit(OpCodes.Ldloc, contextLocal);
        il.Emit(OpCodes.Ldfld, contract.FailedField);
        il.Emit(OpCodes.Brtrue, fallback);

        il.BeginExceptionBlock();

        il.Emit(OpCodes.Ldloc, contextLocal);
        il.Emit(OpCodes.Ldfld, contract.HandlerField);
        il.Emit(OpCodes.Castclass, handlerType);
        EmitArguments(il, parameters.Length);
        il.Emit(OpCodes.Callvirt, invoke);

        EmitStore(il, result);

        il.Emit(OpCodes.Leave, done);

        il.BeginCatchBlock(typeof(Exception));

        Label blamed = il.DefineLabel();

        il.Emit(OpCodes.Stloc, error);
        il.Emit(OpCodes.Ldloc, contextLocal);
        il.Emit(OpCodes.Ldloc, error);
        il.Emit(OpCodes.Callvirt, contract.HandleErrorMethod);
        il.Emit(OpCodes.Brtrue, blamed);

        il.Emit(OpCodes.Rethrow);

        il.MarkLabel(blamed);
        il.Emit(OpCodes.Leave, fallback);

        il.EndExceptionBlock();

        il.MarkLabel(fallback);

        if (origInvoke != null)
        {
            EmitArguments(il, parameters.Length);
            il.Emit(OpCodes.Callvirt, origInvoke);

            EmitStore(il, result);
        }
        else
        {
            EmitDefault(il, result, returnType);
        }

        il.MarkLabel(done);

        EmitReturn(il, result);

        return method.CreateDelegate(handlerType);
    }

    public static Delegate? WrapEmitted(Type handlerType, IEmittedGuard guard, string methodName)
    {
        if (handlerType.GetMethod("Invoke") is not { } invoke)
        {
            return null;
        }

        ParameterInfo[] parameters = invoke.GetParameters();
        Type returnType = invoke.ReturnType;

        Type[] argumentTypes = [typeof(IEmittedGuard), .. parameters.Select(parameter => parameter.ParameterType)];

        DynamicMethod method = new(methodName, returnType, argumentTypes, typeof(GuardEmitter), true);
        ILGenerator il = method.GetILGenerator();

        LocalBuilder? result = returnType == typeof(void) ? null : il.DeclareLocal(returnType);
        LocalBuilder error = il.DeclareLocal(typeof(Exception));
        LocalBuilder handler = il.DeclareLocal(typeof(Delegate));
        Label fallback = il.DefineLabel();
        Label empty = il.DefineLabel();
        Label done = il.DefineLabel();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, _shouldRunMethod);
        il.Emit(OpCodes.Brfalse, fallback);

        il.BeginExceptionBlock();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, _bodyGetter);
        il.Emit(OpCodes.Castclass, handlerType);
        EmitArguments(il, parameters.Length, 1);
        il.Emit(OpCodes.Callvirt, invoke);

        EmitStore(il, result);

        il.Emit(OpCodes.Leave, done);

        il.BeginCatchBlock(typeof(Exception));

        il.Emit(OpCodes.Stloc, error);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldloc, error);
        il.Emit(OpCodes.Callvirt, _markFailedMethod);
        il.Emit(OpCodes.Leave, fallback);

        il.EndExceptionBlock();

        il.MarkLabel(fallback);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, _fallbackGetter);
        il.Emit(OpCodes.Stloc, handler);
        il.Emit(OpCodes.Ldloc, handler);
        il.Emit(OpCodes.Brfalse, empty);

        il.Emit(OpCodes.Ldloc, handler);
        il.Emit(OpCodes.Castclass, handlerType);
        EmitArguments(il, parameters.Length, 1);
        il.Emit(OpCodes.Callvirt, invoke);

        EmitStore(il, result);

        il.Emit(OpCodes.Br, done);

        il.MarkLabel(empty);

        EmitDefault(il, result, returnType);

        il.MarkLabel(done);

        EmitReturn(il, result);

        return method.CreateDelegate(handlerType, guard);
    }

    private static MethodInfo? FindOriginalInvoke(ParameterInfo[] parameters, Type returnType)
    {
        if (parameters.Length == 0 || !typeof(Delegate).IsAssignableFrom(parameters[0].ParameterType))
        {
            return null;
        }

        if (parameters[0].ParameterType.GetMethod("Invoke") is not { } origInvoke || origInvoke.ReturnType != returnType)
        {
            return null;
        }

        return origInvoke.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameters.Skip(1).Select(parameter => parameter.ParameterType)) ? origInvoke : null;
    }

    private static void EmitArguments(ILGenerator il, int count, int offset = 0)
    {
        for (int i = offset; i < count + offset; ++i)
        {
            switch (i)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);

                    break;

                case 1:
                    il.Emit(OpCodes.Ldarg_1);

                    break;

                case 2:
                    il.Emit(OpCodes.Ldarg_2);

                    break;

                case 3:
                    il.Emit(OpCodes.Ldarg_3);

                    break;

                case <= byte.MaxValue:
                    il.Emit(OpCodes.Ldarg_S, (byte)i);

                    break;

                default:
                    il.Emit(OpCodes.Ldarg, (short)i);

                    break;
            }
        }
    }

    private static void EmitStore(ILGenerator il, LocalBuilder? result)
    {
        if (result != null)
        {
            il.Emit(OpCodes.Stloc, result);
        }
    }

    private static void EmitDefault(ILGenerator il, LocalBuilder? result, Type returnType)
    {
        if (result == null)
        {
            return;
        }

        if (returnType.IsValueType)
        {
            il.Emit(OpCodes.Ldloca, result);
            il.Emit(OpCodes.Initobj, returnType);

            return;
        }

        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Stloc, result);
    }

    private static void EmitReturn(ILGenerator il, LocalBuilder? result)
    {
        if (result != null)
        {
            il.Emit(OpCodes.Ldloc, result);
        }

        il.Emit(OpCodes.Ret);
    }
}
