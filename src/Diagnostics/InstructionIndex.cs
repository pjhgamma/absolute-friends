using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;

namespace RippleFriends.Diagnostics;

internal static class InstructionIndex
{
    public static Dictionary<Instruction, int> Build(IList<Instruction> instructions)
    {
        Dictionary<Instruction, int> indices = new(instructions.Count, ReferenceComparer.Instance);

        for (int i = 0; i < instructions.Count; ++i)
        {
            indices[instructions[i]] = i;
        }

        return indices;
    }

    private sealed class ReferenceComparer : IEqualityComparer<Instruction>
    {
        public static readonly ReferenceComparer Instance = new();

        public bool Equals(Instruction? x, Instruction? y) => ReferenceEquals(x, y);

        public int GetHashCode(Instruction obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
