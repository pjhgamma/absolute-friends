using System.Reflection;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.Addons;

public sealed class Addon
{
    private static ModManager.Mod? _host;

    private readonly List<Configurable<bool>> _switches = [];

    private readonly List<string> _keywords = [];

    private readonly Dictionary<string, ConfigurableBase> _options = [];

    private bool _isRunning;

    internal Addon(AddonPlugin plugin, Action<MenuBuilder> buildMenu)
    {
        ModManager.Mod? mod = Owner(plugin.Info.Location);

        _host ??= Owner(typeof(Addon).Assembly.Location);

        Plugin = plugin;
        Id = plugin.Info.Metadata.GUID;
        Name = plugin.Info.Metadata.Name;
        Version = plugin.Info.Metadata.Version.ToString();
        Author = mod?.authors ?? "";
        AbsoluteFriendsVersion = plugin.AbsoluteFriendsVersion;
        AddonDependencyIds = plugin.AddonDependencies;
        IsBuiltIn = mod != null && _host != null && mod.id == _host.id;
        AddonDescription = plugin.AddonDescription;
        BuildMenu = buildMenu;
        HasMenu = plugin.GetType().GetMethod("BuildMenu", BindingFlags.Instance | BindingFlags.NonPublic)?.DeclaringType != typeof(AddonPlugin);
        Prefix = string.Concat(Id.Select(character => char.IsLetterOrDigit(character) ? character : '_'));
        Enabled = RemixMenu.Instance.Bind($"{Prefix}_Addon", true, new ConfigurableInfo(AddonDescription, tags: [Name]));
    }

    internal Addon(ModManager.Mod mod)
    {
        Id = mod.id;
        Name = mod.LocalizedName;
        Version = mod.version;
        Author = mod.authors == ModManager.Mod.authorBlank ? "" : mod.authors;
        AbsoluteFriendsVersion = "";
        AddonDependencyIds = [];
        IsBuiltIn = false;
        AddonDescription = mod.LocalizedDescription;
        BuildMenu = _ => { };
        Prefix = string.Concat(Id.Select(character => char.IsLetterOrDigit(character) ? character : '_'));

        _keywords.Add(mod.name);
    }

    public string Id { get; }

    public string Name { get; }

    public string Version { get; }

    public string Author { get; }

    public string AbsoluteFriendsVersion { get; }

    public bool IsBuiltIn { get; }

    public bool IsApplied => Plugin != null;

    public bool IsMismatched => AbsoluteFriendsVersion.Length > 0 && AbsoluteFriendsVersion != AbsoluteFriends.Plugin.Version;

    public string AddonDescription { get; }

    public IReadOnlyList<string> AddonDependencyIds { get; }

    public bool AddonDependenciesApplied => AddonDependencyIds.All(id => AddonRegistry.Find(id)?.IsApplied == true);

    public int OptionCount => _switches.Count;

    public int EnabledOptionCount => _switches.Count(IsSelected);

    public Configurable<bool>? Enabled { get; }

    internal IEnumerable<string> Keywords => [Name, .. AddonDependencyIds.Select(AddonRegistry.RawNameOf), .. _keywords];

    internal bool HasMenu { get; }

    internal Action<MenuBuilder> BuildMenu { get; }

    internal IReadOnlyDictionary<string, ConfigurableBase> ProfileOptions => _options;

    private AddonPlugin? Plugin { get; }

    private string Prefix { get; }

    private Assembly? Assembly => Plugin?.GetType().Assembly;

    public Configurable<T> Bind<T>(string key, T defaultValue, ConfigurableInfo? info = null)
    {
        Configurable<T> option = RemixMenu.Instance.Bind($"{Prefix}_{key}", defaultValue, info).Require(Enabled!);

        _options[key] = option;

        if (option is Configurable<bool> boolOption && !_switches.Contains(boolOption))
        {
            _switches.Add(boolOption);
        }

        foreach (var tag in info?.Tags ?? [])
        {
            if (tag is string keyword && keyword.Length > 0 && !_keywords.Contains(keyword))
            {
                _keywords.Add(keyword);
            }
        }

        return option;
    }

    internal bool HasOption(string query)
    {
        return _keywords.Any(keyword => keyword.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    internal void UpdateRunningState() => SetRunning(Enabled?.IsActive == true && AddonDependencyIds.All(AddonRegistry.IsRunning));

    internal void Stop() => SetRunning(false);

    private static ModManager.Mod? Owner(string location)
    {
        try
        {
            string path = Storage.NormalizePath(location);

            return ModManager.ActiveMods.FirstOrDefault(mod => path.StartsWith(Storage.NormalizePath(mod.path) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsSelected(Configurable<bool> option)
    {
        bool selected = option.BoundUIconfig is { } control && bool.TryParse(control.value, out bool value) ? value : option.Value;

        return selected && option.Requirements.All(group => group.Any(IsSelected));
    }

    private void SetRunning(bool isRunning)
    {
        if (_isRunning == isRunning)
        {
            return;
        }

        _isRunning = isRunning;

        if (isRunning)
        {
            HookManager.Register(Assembly!);

            Plugin!.Activate();
        }
        else
        {
            Plugin!.Deactivate();

            HookManager.Unregister(Assembly!);
        }
    }
}
