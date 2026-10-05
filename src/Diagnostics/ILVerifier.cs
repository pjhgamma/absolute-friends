using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace AbsoluteFriends.Diagnostics;

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
