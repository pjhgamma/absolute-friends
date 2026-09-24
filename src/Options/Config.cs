namespace AbsoluteFriends.Options;

public static class Config
{
    internal const string NoProfile = "_noProfile";

    internal static Configurable<string> ProfileSelection = null!;

    internal static Configurable<bool> Debug = null!;

    internal static Configurable<bool> HookBaselines = null!;

    private static readonly Dictionary<ConfigurableBase, List<Configurable<bool>[]>> _requirements = [];

    public static string JollyCoop(string text, string jollyCoopText) => ModManager.JollyCoop ? jollyCoopText : text;

    public static string Downpour(string text, string downpourText) => ModManager.MSC ? downpourText : text;

    public static string Watcher(string text, string watcherText) => ModManager.Watcher ? watcherText : text;

    extension(ConfigurableBase? configurableBase)
    {
        public string? Label => configurableBase?.info?.Tags is { Length: > 0 } tags ? tags[0] as string : null;

        public string? Description => configurableBase?.info?.description;

        public IEnumerable<Configurable<bool>[]> Requirements => configurableBase != null && _requirements.TryGetValue(configurableBase, out List<Configurable<bool>[]> groups) ? groups : [];

        private bool IsAllowed => configurableBase.Requirements.All(group => group.Any(master => master.IsActive));
    }

    extension(Configurable<bool>? option)
    {
        public bool IsActive => option is { Value: true } && option.IsAllowed;
    }

    extension<T>(T target) where T : ConfigurableBase
    {
        public T Require(params Configurable<bool>[] masters)
        {
            foreach (var master in masters)
            {
                target.RequireAny(master);
            }

            return target;
        }

        public T RequireAny(params Configurable<bool>[] masters)
        {
            Configurable<bool>[] group = [.. masters.Where(master => master != null)];

            if (group.Length == 0)
            {
                return target;
            }

            if (!_requirements.TryGetValue(target, out List<Configurable<bool>[]> groups))
            {
                _requirements[target] = groups = [];
            }

            if (!groups.Any(registered => registered.SequenceEqual(group)))
            {
                groups.Add(group);
            }

            return target;
        }
    }

    internal static void Bind(OptionInterface oi)
    {
        ProfileSelection = oi.config.Bind("ProfileSelection", NoProfile, new ConfigurableInfo("Selects a profile to load or modify.", tags: ["Selected Profile"]));

        Debug = oi.config.Bind("Debug", true, new ConfigurableInfo("Records patch locations and behavior in the diagnostics report.", tags: ["Debug"]));
        HookBaselines = oi.config.Bind("HookBaselines", false, new ConfigurableInfo("Records hook baselines as HookTest attributes in the diagnostics report.", tags: ["Record Hook Baselines"])).Require(Debug);
    }
}
