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
    public readonly int Index = index;

    public readonly int Inserted = inserted;

    public readonly string Anchor = anchor;

    public bool Rewritten => Inserted == 0;
}

internal sealed class ILSnapshot
{
    private const int AnchorWindow = 3;

    private static readonly string[] _anchorSeparator = ["; "];

    private readonly Instruction[] _instructions;

    private readonly (OpCode OpCode, object? Operand)[] _originals;

    private readonly Dictionary<Instruction, int> _indices;

    private readonly int _variableCount;

    public ILSnapshot(IList<Instruction> instructions, int variableCount)
    {
        _instructions = [.. instructions];
        _originals = [.. _instructions.Select(instruction => (instruction.OpCode, instruction.Operand))];
        _indices = InstructionIndex.Build(_instructions);
        _variableCount = variableCount;
    }

    public int Count => _instructions.Length;

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

        if (recordedAnchors is { Length: > 0 } && !sites.Select(site => Normalize(site.Anchor)).SequenceEqual(recordedAnchors.Select(Normalize)))
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
                else if (IsRewritten(instruction, index))
                {
                    sites.Add(new(index, 0, DescribeAnchor(index)));
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

    private static string Normalize(string anchor) => string.Join("; ", anchor.Split(_anchorSeparator, StringSplitOptions.None).Select(NormalizeOpCode));

    private static string NormalizeOpCode(string part)
    {
        int space = part.IndexOf(' ');
        string name = space < 0 ? part : part.Substring(0, space);

        if (!name.EndsWith(".s", StringComparison.Ordinal))
        {
            return part;
        }

        name = name.Substring(0, name.Length - 2);

        return space < 0 ? name : name + part.Substring(space);
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

    private bool IsRewritten(Instruction instruction, int index)
    {
        var (opCode, operand) = _originals[index];

        return instruction.OpCode != opCode || !Equals(instruction.Operand, operand);
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
}

internal sealed class ILPatchResult(string fullName, ILPatchStatus status, int shift, int unreachable, List<ILPatchSite> sites, List<string> details)
{
    public bool IsHealthy => unreachable == 0 && status is ILPatchStatus.Exact or ILPatchStatus.Shifted or ILPatchStatus.Untracked;

    public string Signature => $"{status}/{shift}/{unreachable}/{FormatSites()}";

    public void Report(bool shared = false)
    {
        if (unreachable > 0)
        {
            HookDiagnostics.LogError($"{fullName}: UNREACHABLE ({unreachable} inserted instruction(s) cannot be reached, so this patch is in place but never runs)");
        }

        string summary = $"{fullName}: {FormatStatus(shared)} {FormatSites()}";

        switch (status)
        {
            case ILPatchStatus.Rearranged:
            case ILPatchStatus.CountMismatch or ILPatchStatus.AnchorMismatch when shared:
                HookDiagnostics.LogWarning(summary);

                break;

            case ILPatchStatus.NotFound or ILPatchStatus.CountMismatch or ILPatchStatus.AnchorMismatch:
                HookDiagnostics.LogError(summary);

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

        HookDiagnostics.LogDetail(details);
    }

    private string FormatStatus(bool shared) => status switch
    {
        ILPatchStatus.Exact => "EXACT",
        ILPatchStatus.Shifted => $"SHIFTED ({shift:+#;-#;0}; another mod most likely patched this method first, but every pattern matched the recorded instruction in the same order)",
        ILPatchStatus.Rearranged => "REARRANGED (the same number of patterns matched, but not at a uniform offset)",
        ILPatchStatus.CountMismatch when shared => "COUNT MISMATCH (this mod patches this method more than once, so the recorded count includes the other patches)",
        ILPatchStatus.CountMismatch => "COUNT MISMATCH (a pattern matched a different number of times than recorded)",
        ILPatchStatus.AnchorMismatch when shared => "ANCHOR MISMATCH (this mod patches this method more than once, so the recorded instruction is whichever patch landed here first)",
        ILPatchStatus.AnchorMismatch => "ANCHOR MISMATCH (the patch landed on different instructions than were recorded, so the game's code has changed underneath it)",
        ILPatchStatus.NotFound => "NOT FOUND (nothing was inserted and nothing was rewritten, so this hook currently does nothing)",
        _ => "UNTRACKED (this hook has no recorded indices to compare against)",
    };

    private string FormatSites() => $"sites=[{string.Join(", ", sites.Select(site => site.Rewritten ? $"{site.Index}*" : $"{site.Index}+{site.Inserted}"))}]";
}

internal static class ILVerifier
{
    public static ILSnapshot Capture(ILContext il) => new(il.Instrs, il.Body?.Variables?.Count ?? 0);

    public static ILPatchResult Verify(string fullName, ILSnapshot snapshot, ILContext il, int[] recordedIndices, string[] recordedAnchors)
    {
        List<ILPatchSite> sites = snapshot.FindPatchSites(il.Instrs);
        ILPatchStatus status = ILSnapshot.Classify(sites, recordedIndices, recordedAnchors, out int shift);

        int unreachable = 0;

        if (il.Body is { } body)
        {
            foreach (var index in ILReachability.FindUnreachable(body, BranchTargets))
            {
                if (index < il.Instrs.Count && !snapshot.Contains(il.Instrs[index]))
                {
                    unreachable++;
                }
            }
        }

        int removed = snapshot.CountRemoved(il.Instrs);
        int added = snapshot.CountAddedVariables(il.Body?.Variables?.Count ?? 0);

        List<string> details = [$"{snapshot.Count} instructions before, {il.Instrs.Count} after ({removed} removed, {added} local(s) added)"];

        if (recordedIndices is { Length: > 0 })
        {
            details.Add($"expected=[{string.Join(", ", recordedIndices)}]");
        }

        for (int i = 0; i < sites.Count; ++i)
        {
            string recorded = recordedAnchors is { Length: > 0 } && i < recordedAnchors.Length && recordedAnchors[i] != sites[i].Anchor ? $" (recorded: {recordedAnchors[i]})" : "";

            string what = sites[i].Rewritten ? "rewritten where it stood:" : $"{sites[i].Inserted} instruction(s) inserted before";

            details.Add($"site {i}: {what} [{sites[i].Index}] {sites[i].Anchor}{recorded}");
        }

        return new(fullName, status, shift, unreachable, sites, details);
    }

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
