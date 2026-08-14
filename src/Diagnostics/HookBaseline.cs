namespace RippleFriends.Diagnostics;

internal static class HookBaseline
{
#if RIPPLEFRIENDS_RECORDBASELINE

    public const bool Enabled = true;

    private static readonly Dictionary<string, string> _entries = [];

    public static void Begin() => _entries.Clear();

    public static void Record(string fullName, string attribute) => _entries[fullName] = attribute;

    public static void Announce()
    {
        if (_entries.Count == 0)
        {
            return;
        }

        HookDiagnostics.LogInfo($"Hook baseline for {_entries.Count} IL hook(s) recorded at the end of the diagnostics report");

        if (!ModManager.MSC)
        {
            HookDiagnostics.LogWarning("Downpour is not enabled, so no Downpour hook was recorded. This baseline is incomplete.");
        }

        if (!ModManager.Watcher)
        {
            HookDiagnostics.LogWarning("Watcher is not enabled, so no Watcher hook was recorded. This baseline is incomplete.");
        }
    }

    public static List<string> Lines
    {
        get
        {
            List<string> lines = [];

            foreach (var entry in _entries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                lines.Add($"// {entry.Key}");
                lines.Add(entry.Value);
            }

            return lines;
        }
    }

#else

    public const bool Enabled = false;

    public static List<string> Lines => [];

    public static void Begin()
    {
    }

    public static void Record(string fullName, string attribute)
    {
    }

    public static void Announce()
    {
    }

#endif
}
