using RippleFriends.Options;
using System.Reflection;
using System.Text;

namespace RippleFriends.Diagnostics;

internal static class HookReport
{
    public static string SetHeading(this string text, int level) => $"{new string('#', level)} {text}";

    public static string SetItem(this string text) => $"- {text}";

    public static string SetBold(this string text) => $"**{text}**";

    public static string SetCode(this string text) => $"`{text}`";

    public static string SetCode(this IEnumerable<string> values) => string.Join(", ", values.Select(SetCode));

    public static string Fence => "```";

    public static void AppendHeading(this StringBuilder builder, string heading, int level)
    {
        builder.AppendLine(heading.SetHeading(level)).AppendLine();
    }

    public static void AppendLines(this StringBuilder builder, List<string> lines)
    {
        foreach (var line in lines)
        {
            builder.AppendLine(line);
        }

        builder.AppendLine();
    }

    public static void AppendSection(this StringBuilder builder, string heading, List<string> lines, string language = "")
    {
        if (lines.Count == 0)
        {
            return;
        }

        builder.AppendHeading(heading, 2);
        builder.AppendBlock(lines, language);
    }

    public static void AppendBlock(this StringBuilder builder, List<string> lines, string language = "")
    {
        if (lines.Count == 0)
        {
            return;
        }

        builder.AppendLine(Fence + language);

        foreach (var line in lines)
        {
            builder.AppendLine(line);
        }

        builder.AppendLine(Fence).AppendLine();
    }
}

internal static class HookDiagnostics
{
    private const string ReportFileName = "RippleFriends_Diagnostics.md";

    private static readonly List<string> _report = [];

    private static readonly List<string> _problems = [];

    private static int _errorCount;

    private static int _warningCount;

    private static bool _writing;

    public static void BeginSession()
    {
        _report.Clear();
        _problems.Clear();
        _errorCount = 0;
        _warningCount = 0;

        HookBaseline.Begin();
    }

    public static void EndSession()
    {
        HookBaseline.Announce();

        WriteReport();
    }

    public static void Log(string message)
    {
        UnityEngine.Debug.Log("Ripple Friends: " + message);
    }

    public static void LogInfo(string message)
    {
        Record("[INFO] " + message);
    }

    public static void LogDetail(List<string> lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        foreach (var line in lines)
        {
            Record("    " + line);
        }
    }

    public static void LogError(string message)
    {
        _errorCount++;

        Flag("[ERROR] " + message);

        WriteReport();
    }

    public static void LogError(string message, Exception exception)
    {
        LogError($"{message}: {exception.Message}");
        Record(exception.ToString());
    }

    public static void LogWarning(string message)
    {
        _warningCount++;

        Flag("[WARN] " + message);
    }

    public static void LogWarning(string message, Exception exception)
    {
        LogWarning($"{message}: {exception.Message}");
        Record(exception.ToString());
    }

    public static void CopyReport()
    {
        string report = BuildReport();

        CopyToClipboard(report);
        Save(report);
    }

    private static void Record(string message)
    {
        if (Config.Debug.Value)
        {
            _report.Add(message);
        }
    }

    private static void Flag(string message)
    {
        _problems.Add(message);
        Record(message);
        Log(message);
    }

    private static string? Save(string report)
    {
        try
        {
            string path = Path.Combine(RWCustom.Custom.RootFolderDirectory(), ReportFileName);

            File.WriteAllText(path, report);

            return path;
        }
        catch (Exception exception)
        {
            LogError("Could not write report", exception);

            return null;
        }
    }

    private static string BuildReport()
    {
        StringBuilder builder = new();

        builder.AppendHeading($"{Plugin.Name} {Plugin.Version} diagnostics", 1);
        builder.AppendLines(Provenance);

        builder.AppendSection("Hook baseline", HookBaseline.Lines, "csharp");

        if (_report.Count == 0)
        {
            builder.AppendLine("Nothing was recorded.");

            return builder.ToString();
        }

        builder.AppendSection($"{_errorCount} error(s), {_warningCount} warning(s)", _problems);
        builder.AppendSection("Full log", _report);
        builder.AppendSection("Hooks that ran", Activities, "csharp");
        builder.AppendSection("Hooks that never ran", Silent, "csharp");

        return builder.ToString();
    }

    private static void WriteReport()
    {
        if (_writing || !Config.Debug.Value)
        {
            return;
        }

        _writing = true;

        try
        {
            Save(BuildReport());
        }
        finally
        {
            _writing = false;
        }
    }

    private static void CopyToClipboard(string report)
    {
        try
        {
            UnityEngine.GUIUtility.systemCopyBuffer = report;
        }
        catch (Exception exception)
        {
            LogError($"Could not reach the clipboard", exception);
        }
    }

    private static string GameVersion
    {
        get
        {
            try
            {
                return RainWorld.GAME_VERSION_STRING;
            }
            catch
            {
                return "unknown";
            }
        }
    }

    private static string Mods
    {
        get
        {
            try
            {
                if (ModManager.ActiveMods is not { } mods)
                {
                    return "unknown".SetCode();
                }

                return mods.Select(mod => $"{mod.id} {mod.version}").SetCode();
            }
            catch
            {
                return "unknown".SetCode();
            }
        }
    }

    private static string Options
    {
        get
        {
            List<string> options = [];

            try
            {
                foreach (var member in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (member.GetValue(null) is not ConfigurableBase configurable)
                    {
                        continue;
                    }

                    string value = configurable.BoxedValue?.ToString() ?? "?";
                    bool changed = !string.Equals(value, configurable.defaultValue, StringComparison.OrdinalIgnoreCase);

                    options.Add($"{configurable.key}={value}{(changed ? "*" : "")}");
                }
            }
            catch (Exception exception)
            {
                return $"unreadable ({exception.Message})";
            }

            return options.SetCode();
        }
    }

    private static List<string> Provenance
    {
        get
        {
            try
            {
                return
                [
                    $"{"game".SetBold()}: {GameVersion.SetCode()}".SetItem(),
                    $"{"mods".SetBold()}: {Mods}".SetItem(),
                    $"{"options".SetBold()}: {Options}".SetItem(),
                ];
            }
            catch (Exception exception)
            {
                return [$"unreadable ({exception.Message})".SetItem()];
            }
        }
    }

    private static List<string> Activities => BindingNames(binding => binding.Ran);

    private static List<string> Silent => BindingNames(binding => !binding.Ran);

    private static List<string> BindingNames(Func<Hooks.HookBinding, bool> match)
    {
        List<string> names = [];

        try
        {
            foreach (var binding in Hooks.HookManager.Bindings)
            {
                if (match(binding))
                {
                    names.Add(binding.FullName);
                }
            }
        }
        catch (Exception exception)
        {
            names.Add($"unreadable ({exception.Message})");
        }

        return names;
    }
}
