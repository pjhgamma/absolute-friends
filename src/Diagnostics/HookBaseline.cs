using RippleFriends.Options;

namespace RippleFriends.Diagnostics;

internal static class HookBaseline
{
    private static readonly Dictionary<string, string> _entries = [];

    public static bool Enabled => Config.HookBaselines.IsActive;

    public static List<string> Lines
    {
        get
        {
            if (!Enabled)
            {
                return [];
            }

            List<string> lines = [];

            foreach (var entry in _entries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                lines.Add($"// {entry.Key}");
                lines.Add(entry.Value);
            }

            return lines;
        }
    }

    public static void Begin() => _entries.Clear();

    public static void Record(string fullName, string attribute)
    {
        if (Enabled)
        {
            _entries[fullName] = attribute;
        }
    }

    public static void Announce()
    {
        if (!Enabled || _entries.Count == 0)
        {
            return;
        }

        Reporter.LogInfo($"Hook baseline for {_entries.Count} IL hook(s) recorded at the end of the diagnostics report");

        if (!ModManager.MSC)
        {
            Reporter.LogWarning("Downpour is not enabled, so no Downpour hook was recorded. This baseline is incomplete.");
        }

        if (!ModManager.Watcher)
        {
            Reporter.LogWarning("Watcher is not enabled, so no Watcher hook was recorded. This baseline is incomplete.");
        }
    }
}
