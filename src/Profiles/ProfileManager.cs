using RippleFriends.Addons;
using RippleFriends.Diagnostics;
using RippleFriends.Options;

namespace RippleFriends.Profiles;

internal static class ProfileManager
{
    internal static (Profile Profile, ProfileResult Result) Create(string name)
    {
        string now = Timestamp();
        Profile profile = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = NormalizeName(name),
            CreatedAt = now,
            ModifiedAt = now,
        };

        ProfileResult result = Capture(profile);
        ProfileStore.Save(profile);

        return (profile, result);
    }

    internal static Profile Copy(Profile source, string name)
    {
        string now = Timestamp();
        Profile profile = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = NormalizeName(name),
            CreatedAt = now,
            ModifiedAt = now,
            Addons = source.Addons.ToDictionary(
                pair => pair.Key,
                pair => new ProfileAddon
                {
                    Name = pair.Value.Name,
                    Version = pair.Value.Version,
                    Enabled = pair.Value.Enabled,
                    Options = new(pair.Value.Options),
                }
            ),
        };

        ProfileStore.Save(profile);

        return profile;
    }

    internal static ProfileResult Update(Profile profile)
    {
        ProfileResult result = Capture(profile);

        profile.ModifiedAt = Timestamp();
        ProfileStore.Save(profile);

        return result;
    }

    internal static void Rename(Profile profile, string name)
    {
        profile.Name = NormalizeName(name);
        profile.ModifiedAt = Timestamp();

        ProfileStore.Save(profile);
    }

    internal static ProfileResult Apply(Profile profile)
    {
        int addons = 0;
        int options = 0;
        int unavailableAddons = 0;
        int unavailableOptions = 0;

        foreach (var pair in profile.Addons)
        {
            Addon? addon = AddonRegistry.Find(pair.Key);

            if (addon == null)
            {
                unavailableAddons++;
                unavailableOptions += pair.Value.Options.Count(option => IsBoolean(null, option));

                continue;
            }

            addons++;
            Set(addon.Enabled, pair.Value.Enabled, ref unavailableOptions);
        }

        foreach (var pair in profile.Addons)
        {
            Addon? addon = AddonRegistry.Find(pair.Key);

            if (addon == null)
            {
                continue;
            }

            foreach (var option in pair.Value.Options)
            {
                if (!addon.ProfileOptions.TryGetValue(option.Key, out ConfigurableBase configurable))
                {
                    if (IsBoolean(null, option))
                    {
                        unavailableOptions++;
                    }

                    continue;
                }

                int before = unavailableOptions;
                Set(configurable, option.Value, ref unavailableOptions);

                if (configurable is Configurable<bool> && before == unavailableOptions)
                {
                    options++;
                }
            }
        }

        return new(addons, options, unavailableAddons, unavailableOptions);
    }

    internal static void Remove(Profile profile) => ProfileStore.Remove(profile);

    internal static ProfileSummary Summarize(Profile profile)
    {
        int enabledAddons = profile.Addons.Count(pair => AddonRegistry.Find(pair.Key) is { Enabled: { } enabled } && IsEnabled(profile, enabled));
        int enabledOptions = 0;
        int options = 0;
        int unavailableAddons = 0;
        int unavailableOptions = 0;

        foreach (var pair in profile.Addons)
        {
            Addon? addon = AddonRegistry.Find(pair.Key);

            if (addon == null)
            {
                unavailableAddons++;
                unavailableOptions += pair.Value.Options.Count(option => IsBoolean(null, option));
            }

            foreach (var option in pair.Value.Options)
            {
                if (IsBoolean(addon, option))
                {
                    options++;

                    if (addon?.ProfileOptions.TryGetValue(option.Key, out ConfigurableBase configurable) == true && IsEnabled(profile, configurable))
                    {
                        enabledOptions++;
                    }
                }
            }

            if (addon != null)
            {
                unavailableOptions += pair.Value.Options.Count(option => !addon.ProfileOptions.ContainsKey(option.Key) && IsBoolean(null, option));
            }
        }

        return new(enabledAddons, profile.Addons.Count, enabledOptions, options, unavailableAddons, unavailableOptions);
    }

    internal static string State()
    {
        System.Text.StringBuilder state = new();

        foreach (Addon addon in AddonRegistry.Applied)
        {
            Append(state, addon.Id);
            Append(state, ValueOf(addon.Enabled));

            foreach (var option in addon.ProfileOptions)
            {
                Append(state, option.Key);
                Append(state, ValueOf(option.Value));
            }
        }

        return state.ToString();
    }

    private static ProfileResult Capture(Profile profile)
    {
        int addons = 0;
        int options = 0;

        foreach (Addon addon in AddonRegistry.Applied)
        {
            if (!profile.Addons.TryGetValue(addon.Id, out ProfileAddon saved))
            {
                profile.Addons[addon.Id] = saved = new();
            }

            saved.Name = addon.Name;
            saved.Version = addon.Version;
            saved.Enabled = ValueOf(addon.Enabled);

            foreach (var option in addon.ProfileOptions)
            {
                saved.Options[option.Key] = ValueOf(option.Value);

                if (option.Value is Configurable<bool>)
                {
                    options++;
                }
            }

            addons++;
        }

        int unavailableAddons = profile.Addons.Keys.Count(id => AddonRegistry.Find(id) == null);
        int unavailableOptions = profile.Addons.Sum(pair =>
        {
            Addon? addon = AddonRegistry.Find(pair.Key);

            return addon == null
                ? pair.Value.Options.Count(option => IsBoolean(null, option))
                : pair.Value.Options.Count(option => !addon.ProfileOptions.ContainsKey(option.Key) && IsBoolean(null, option));
        });

        return new(addons, options, unavailableAddons, unavailableOptions);
    }

    private static string NormalizeName(string name)
    {
        string normalized = name.Trim();

        return normalized.Length switch
        {
            0 => "Profile",
            > 80 => normalized.Substring(0, 80),
            _ => normalized,
        };
    }

    private static string Timestamp() => $"{DateTime.UtcNow:O}";

    private static bool IsTrue(string value) => bool.TryParse(value, out bool enabled) && enabled;

    private static bool IsBoolean(Addon? addon, KeyValuePair<string, string> option)
    {
        return addon?.ProfileOptions.TryGetValue(option.Key, out ConfigurableBase configurable) == true
            ? configurable is Configurable<bool>
            : bool.TryParse(option.Value, out _);
    }

    private static bool IsEnabled(Profile profile, ConfigurableBase configurable)
    {
        return IsTrue(ProfileValue(profile, configurable)) && configurable.Requirements.All(group => group.Any(master => IsEnabled(profile, master)));
    }

    private static string ProfileValue(Profile profile, ConfigurableBase configurable)
    {
        foreach (Addon addon in AddonRegistry.Applied)
        {
            if (!profile.Addons.TryGetValue(addon.Id, out ProfileAddon saved))
            {
                continue;
            }

            if (addon.Enabled == configurable)
            {
                return saved.Enabled;
            }

            foreach (var option in addon.ProfileOptions)
            {
                if (option.Value == configurable && saved.Options.TryGetValue(option.Key, out string value))
                {
                    return value;
                }
            }
        }

        return "false";
    }

    private static void Append(System.Text.StringBuilder state, string value)
    {
        state.Append(value.Length).Append(':').Append(value);
    }

    private static string ValueOf(ConfigurableBase? configurable)
    {
        return configurable?.BoundUIconfig?.value ?? configurable?.BoxedValue?.ToString() ?? "";
    }

    private static void Set(ConfigurableBase? configurable, string value, ref int unavailable)
    {
        try
        {
            if (configurable?.BoundUIconfig is not { } control)
            {
                unavailable++;

                return;
            }

            control.value = value;

            if (!string.Equals(control.value, value, StringComparison.OrdinalIgnoreCase))
            {
                unavailable++;
            }
        }
        catch (Exception exception)
        {
            unavailable++;
            Reporter.LogError($"Could not apply profile option {configurable?.key}", exception);
        }
    }
}
