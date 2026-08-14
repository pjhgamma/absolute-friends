using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Text;

namespace RippleFriends.Diagnostics;

internal enum ILPatchStatus
{
    NotFound,
    Exact,
    Shifted,
    Rearranged,
    CountMismatch,
    AnchorMismatch,
    Untracked,
}

internal readonly struct ILPatchSite(int index, int inserted, string anchor)
{
    public readonly int Inserted = inserted;

    public readonly int Index = index;

    public readonly string Anchor = anchor;
}

internal sealed class ILSnapshot
{
    public int Count => _instructions.Length;

    private const int AnchorWindow = 3;

    private readonly Instruction[] _instructions;

    private readonly Dictionary<Instruction, int> _indices;

    private readonly int _variableCount;

    public ILSnapshot(IList<Instruction> instructions, int variableCount)
    {
        _instructions = [.. instructions];
        _indices = InstructionIndex.Build(_instructions);
        _variableCount = variableCount;
    }

    public bool Contains(Instruction instruction) => _indices.ContainsKey(instruction);

    public List<ILPatchSite> FindPatchSites(IList<Instruction> patched)
    {
        List<ILPatchSite> sites = [];
        int inserted = 0;

        foreach (var instruction in patched)
        {
            if (_indices.TryGetValue(instruction, out int index))
            {
                if (inserted > 0)
                {
                    sites.Add(new(index, inserted, DescribeAnchor(index)));

                    inserted = 0;
                }
            }
            else
            {
                inserted++;
            }
        }

        if (inserted > 0)
        {
            sites.Add(new(_instructions.Length, inserted, "end of method"));
        }

        return sites;
    }

    public int CountRemoved(IList<Instruction> patched) => _instructions.Length - patched.Count(_indices.ContainsKey);

    public int CountAddedVariables(int variableCount) => variableCount - _variableCount;

    public static ILPatchStatus Classify(List<ILPatchSite> sites, int[] recordedIndices, string[] recordedAnchors, out int shift)
    {
        shift = 0;

        if (sites.Count == 0)
        {
            return ILPatchStatus.NotFound;
        }

        if (recordedIndices == null || recordedIndices.Length == 0)
        {
            return ILPatchStatus.Untracked;
        }

        if (sites.Count != recordedIndices.Length)
        {
            return ILPatchStatus.CountMismatch;
        }

        if (recordedAnchors is { Length: > 0 } && !sites.Select(site => site.Anchor).SequenceEqual(recordedAnchors))
        {
            return ILPatchStatus.AnchorMismatch;
        }

        shift = sites[0].Index - recordedIndices[0];

        if (sites.Select((site, index) => site.Index - recordedIndices[index]).Distinct().Count() > 1)
        {
            return ILPatchStatus.Rearranged;
        }

        return shift == 0 ? ILPatchStatus.Exact : ILPatchStatus.Shifted;
    }

    private string DescribeAnchor(int index)
    {
        StringBuilder builder = new();

        foreach (var instruction in _instructions.Skip(index).Take(AnchorWindow))
        {
            if (builder.Length > 0)
            {
                builder.Append("; ");
            }

            builder.Append(Describe(instruction, out bool named));

            if (named)
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static string Describe(Instruction instruction, out bool named)
    {
        try
        {
            var (description, isNamed) = instruction.Operand switch
            {
                TypeReference type => ($"{instruction.OpCode.Name} {type.Name}", true),
                MemberReference member => ($"{instruction.OpCode.Name} {member.DeclaringType?.Name}::{member.Name}", true),
                _ => (instruction.OpCode.Name, false),
            };

            named = isNamed;

            return description;
        }
        catch
        {
            named = false;

            return "?";
        }
    }
}

internal static class ILVerifier
{
    public static ILSnapshot Capture(ILContext il) => new(il.Instrs, il.Body?.Variables?.Count ?? 0);

    public static bool Verify(string fullName, ILSnapshot snapshot, ILContext il, int[] recordedIndices, string[] recordedAnchors)
    {
        List<ILPatchSite> sites = snapshot.FindPatchSites(il.Instrs);
        ILPatchStatus status = ILSnapshot.Classify(sites, recordedIndices, recordedAnchors, out int shift);

        int unreachable = 0;

        if (il.Body is { } body)
        {
            foreach (int index in ILReachability.FindUnreachable(body, BranchTargets))
            {
                if (index < il.Instrs.Count && !snapshot.Contains(il.Instrs[index]))
                {
                    unreachable++;
                }
            }
        }

        if (unreachable > 0)
        {
            HookDiagnostics.LogError($"{fullName}: UNREACHABLE ({unreachable} inserted instruction(s) cannot be reached, so this patch is in place but never runs)");
        }

        string summary = $"{fullName}: {FormatStatus(status, shift)} {FormatSites(sites)}";

        switch (status)
        {
            case ILPatchStatus.NotFound or ILPatchStatus.CountMismatch or ILPatchStatus.AnchorMismatch:
                HookDiagnostics.LogError(summary);

                break;

            case ILPatchStatus.Rearranged:
                HookDiagnostics.LogWarning(summary);

                break;

            default:
                HookDiagnostics.LogInfo(summary);

                break;
        }

#if RIPPLEFRIENDS_RECORDBASELINE
        string attribute = $"[HookTest([{string.Join(", ", sites.Select(site => site.Index))}], [{string.Join(", ", sites.Select(site => $"\"{site.Anchor}\""))}])]";

        HookDiagnostics.LogInfo($"{fullName}: {attribute}");

        HookBaseline.Record(fullName, attribute);
#endif

        List<string> details = [$"{snapshot.Count} instructions before, {il.Instrs.Count} after ({snapshot.CountRemoved(il.Instrs)} removed, {snapshot.CountAddedVariables(il.Body?.Variables?.Count ?? 0)} local(s) added)"];

        if (recordedIndices is { Length: > 0 })
        {
            details.Add($"expected=[{string.Join(", ", recordedIndices)}]");
        }

        for (int i = 0; i < sites.Count; ++i)
        {
            string recorded = recordedAnchors is { Length: > 0 } && i < recordedAnchors.Length && recordedAnchors[i] != sites[i].Anchor ? $" (recorded: {recordedAnchors[i]})" : "";

            details.Add($"site {i}: {sites[i].Inserted} instruction(s) inserted before [{sites[i].Index}] {sites[i].Anchor}{recorded}");
        }

        HookDiagnostics.LogDetail(details);

        return unreachable == 0 && status is ILPatchStatus.Exact or ILPatchStatus.Shifted or ILPatchStatus.Untracked;
    }

    private static string FormatStatus(ILPatchStatus status, int shift) => status switch
    {
        ILPatchStatus.Exact => "EXACT",
        ILPatchStatus.Shifted => $"SHIFTED ({shift:+#;-#;0}; another mod most likely patched this method first, but every pattern matched the recorded instruction in the same order)",
        ILPatchStatus.Rearranged => "REARRANGED (the same number of patterns matched, but not at a uniform offset)",
        ILPatchStatus.CountMismatch => "COUNT MISMATCH (a pattern matched a different number of times than recorded)",
        ILPatchStatus.AnchorMismatch => "ANCHOR MISMATCH (the patch landed on different instructions than were recorded, so the game's code has changed underneath it)",
        ILPatchStatus.NotFound => "NOT FOUND (nothing was inserted, so this hook currently does nothing)",
        _ => "UNTRACKED (this hook has no recorded indices to compare against)",
    };

    private static string FormatSites(List<ILPatchSite> sites) => $"sites=[{string.Join(", ", sites.Select(site => $"{site.Index}+{site.Inserted}"))}]";

    private static IEnumerable<Instruction> BranchTargets(object? operand)
    {
        return operand switch
        {
            ILLabel { Target: { } target } => [target],
            ILLabel[] labels => labels.Where(label => label.Target != null).Select(label => label.Target),
            _ => ILReachability.BranchTargets(operand),
        };
    }
}
