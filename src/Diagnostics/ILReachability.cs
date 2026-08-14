using Mono.Cecil.Cil;

namespace RippleFriends.Diagnostics;

internal static class ILReachability
{
    public static IEnumerable<Instruction> BranchTargets(object? operand)
    {
        return operand switch
        {
            Instruction target => [target],
            Instruction[] targets => targets,
            _ => [],
        };
    }

    public static List<int> FindUnreachable(IList<Instruction> instructions, IEnumerable<int>? extraEntries = null, Func<object?, IEnumerable<Instruction>>? branchTargets = null)
    {
        branchTargets ??= BranchTargets;

        if (instructions.Count == 0)
        {
            return [];
        }

        Dictionary<Instruction, int> indices = InstructionIndex.Build(instructions);

        bool[] reached = new bool[instructions.Count];
        Stack<int> pending = new();

        pending.Push(0);

        if (extraEntries != null)
        {
            foreach (int entry in extraEntries)
            {
                pending.Push(entry);
            }
        }

        while (pending.Count > 0)
        {
            int index = pending.Pop();

            if (index < 0 || index >= instructions.Count || reached[index])
            {
                continue;
            }

            reached[index] = true;

            Instruction instruction = instructions[index];

            foreach (int target in Targets(instruction, indices, branchTargets))
            {
                pending.Push(target);
            }

            if (instruction.OpCode.FlowControl is not (FlowControl.Branch or FlowControl.Return or FlowControl.Throw))
            {
                pending.Push(index + 1);
            }
        }

        return [.. Enumerable.Range(0, reached.Length).Where(index => !reached[index])];
    }

    public static List<int> FindUnreachable(MethodBody body, Func<object?, IEnumerable<Instruction>>? branchTargets = null) => FindUnreachable(body.Instructions, HandlerEntries(body), branchTargets);

    private static IEnumerable<int> HandlerEntries(MethodBody body)
    {
        if (!body.HasExceptionHandlers)
        {
            yield break;
        }

        Dictionary<Instruction, int> indices = InstructionIndex.Build(body.Instructions);

        foreach (ExceptionHandler handler in body.ExceptionHandlers)
        {
            foreach (Instruction? start in new[] { handler.TryStart, handler.HandlerStart, handler.FilterStart })
            {
                if (start != null && indices.TryGetValue(start, out int index))
                {
                    yield return index;
                }
            }
        }
    }

    private static IEnumerable<int> Targets(Instruction instruction, Dictionary<Instruction, int> indices, Func<object?, IEnumerable<Instruction>> branchTargets)
    {
        foreach (Instruction target in branchTargets(instruction.Operand))
        {
            if (indices.TryGetValue(target, out int index))
            {
                yield return index;
            }
        }
    }
}
