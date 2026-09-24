using AbsoluteFriends.Addons;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Options;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AbsoluteFriends.Diagnostics;

internal static class Report
{
    private const string Fence = "```";

    extension(string text)
    {
        public string Item => $"- {text}";

        public string Bold => $"**{text}**";

        public string Code => $"`{text}`";

        public string Heading(int level) => $"{new string('#', level)} {text}";
    }

    extension(IEnumerable<string> values)
    {
        public string Code => string.Join(", ", values.Select(value => value.Code));
    }

    extension(StringBuilder builder)
    {
        public void AppendHeading(string heading, int level)
        {
            builder.AppendLine(heading.Heading(level)).AppendLine();
        }

        public void AppendLines(List<string> lines)
        {
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            builder.AppendLine();
        }

        public void AppendBlock(List<string> lines, string language = "")
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

        public void AppendSection(string heading, List<string> lines, string language = "")
        {
            if (lines.Count == 0)
            {
                return;
            }

            builder.AppendHeading(heading, 2);
            builder.AppendBlock(lines, language);
        }
    }
}

public static class Reporter
{
    public const string ReportFileName = "diagnostics.md";

    private static readonly Dictionary<string, AddonLogger> _loggers = [];

    private static readonly List<string> _report = [];

    private static readonly List<string> _problems = [];

    private static int _errorCount;

    private static int _warningCount;

    public static string ReportPath => Path.Combine(Storage.RootPath, ReportFileName);

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
                    return "unknown".Code;
                }

                return mods.Select(mod => $"{mod.id} {mod.version}").Code;
            }
            catch
            {
                return "unknown".Code;
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

            return options.Code;
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
                    $"{"game".Bold}: {GameVersion.Code}".Item,
                    $"{"mods".Bold}: {Mods}".Item,
                    $"{"options".Bold}: {Options}".Item,
                ];
            }
            catch (Exception exception)
            {
                return [$"unreadable ({exception.Message})".Item];
            }
        }
    }

    private static List<string> ActiveBindings => BindingNames(binding => binding.Ran);

    private static List<string> SilentBindings => BindingNames(binding => !binding.Ran);

    private static List<string> FlaggedBindings => BindingNames(binding => binding.Warned || binding.Failed);

    public static AddonLogger GetLogger(string name)
    {
        if (!_loggers.TryGetValue(name, out AddonLogger logger))
        {
            _loggers[name] = logger = new(name);
        }

        return logger;
    }

    internal static void BeginSession()
    {
        _report.Clear();
        _problems.Clear();
        _errorCount = 0;
        _warningCount = 0;

        HookBaseline.Begin();
    }

    internal static void EndSession()
    {
        HookBaseline.Announce();

        SaveReport();
    }

    internal static void LogConsole(string message)
    {
        Debug.Log("Absolute Friends: " + message);
    }

    internal static void LogDetail(List<string> lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        foreach (var line in lines)
        {
            RecordDetail(line);
        }
    }

    internal static void LogInfo(string message)
    {
        Record("[INFO] " + message);
    }

    internal static void LogWarning(string message)
    {
        _warningCount++;

        Flag("[WARN] " + message);
    }

    internal static void LogWarning(string message, Exception exception)
    {
        LogWarning($"{message}: {exception.Message}");
        LogDetail([exception.ToString()]);
    }

    internal static void LogError(string message)
    {
        RecordError(message);

        SaveReport();
    }

    internal static void LogError(string message, Exception exception)
    {
        RecordError($"{message}: {exception.Message}");
        LogDetail([exception.ToString()]);

        SaveReport();
    }

    internal static void SaveReport()
    {
        if (Config.Debug.IsActive)
        {
            Save(BuildReport());
        }
    }

    internal static void OpenReport()
    {
        SaveReport();

        try
        {
            Storage.Show(ReportPath);
        }
        catch (Exception exception)
        {
            LogError("Could not open the report", exception);
        }
    }

    private static void Record(string message)
    {
        if (Config.Debug.IsActive)
        {
            _report.Add($"{DateTime.UtcNow:T} {message}");
        }
    }

    private static void RecordDetail(string message)
    {
        if (Config.Debug.IsActive)
        {
            _report.Add("    " + message);
        }
    }

    private static void RecordError(string message)
    {
        _errorCount++;

        Flag("[ERROR] " + message);
    }

    private static void Flag(string message)
    {
        _problems.Add(message);
        Record(message);
        LogConsole(message);
    }

    private static List<string> BindingNames(Func<HookBinding, bool> match)
    {
        List<string> names = [];

        try
        {
            foreach (var binding in HookManager.Bindings)
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
        builder.AppendSection("Hooks that were flagged", FlaggedBindings, "csharp");
        builder.AppendSection("Hooks that ran", ActiveBindings, "csharp");
        builder.AppendSection("Hooks that never ran", SilentBindings, "csharp");

        return builder.ToString();
    }

    private static void Save(string report)
    {
        try
        {
            Storage.WriteText(ReportPath, report);
        }
        catch (Exception exception)
        {
            RecordError($"Could not write report: {exception.Message}");
            LogDetail([exception.ToString()]);
        }
    }
}

internal class ReportHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.Debug];

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ctor))]
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        Reporter.SaveReport();
    }

    [HookPatch(typeof(On.RainWorldGame), nameof(On.RainWorldGame.ShutDownProcess))]
    private static void On_RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);

        Reporter.SaveReport();
    }
}
