using Menu.Remix.MixedUI;
using RippleFriends.Diagnostics;
using RippleFriends.Profiles;
using RippleFriends.Utils;

namespace RippleFriends.Options;

internal sealed partial class RemixMenu
{
    private static string ProfileTime(string? value)
    {
        return DateTimeOffset.TryParse(value, out DateTimeOffset time) ? time.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "";
    }

    private void BuildProfiles(OpTab tab)
    {
        BeginTab(tab);
        AddTitle("Profiles");
        AddLabel("Profiles save addon and option settings.");
        AddLabel("Selecting a profile loads it and saves changes immediately; apply to keep them.");

        _profiles.Clear();
        _profiles.AddRange(ProfileStore.Load());
        _profileIndex = _profiles.Count > 0 ? 0 : -1;
        _profileFilter = AddListSearch("_searchProfiles", "Shows only the profiles whose name contains what is typed here.");
        _profileViews.Clear();

        BeginSection();
        AddContainer(delegate
        {
            SetColumns(1);
            _createEditor = AddProfileCreator("Create Profile", "Creates a new profile from the current option values.", "Profile", CreateProfile);
        });

        int profileSlots = Math.Max(_profiles.Count + SpareProfileSlots, MinimumProfileSlots);

        _profileRadioGroup = AddRadioButtonGroup(CreateProfileSelection());
        _profileRadioGroup?.OnValueUpdate += (_, value, _) =>
        {
            if (int.TryParse(value, out int slot))
            {
                SelectProfile(slot);
            }
        };

        for (int slot = 0; slot < profileSlots; slot++)
        {
            _profileViews.Add(BuildProfileView(slot));
        }

        Watch(RefreshProfileViews);
        RefreshProfileViews();
    }

    private Configurable<int> CreateProfileSelection()
    {
        return CreateTransientConfigurable(
            Math.Max(_profileIndex, 0),
            new ConfigurableInfo(Translate("Selects this profile."))
        );
    }

    private ProfileView BuildProfileView(int slot)
    {
        OpRect? container = null;

        BeginSection(gate: () => ProfileMatches(slot));
        BeginBox();

        SetColumns(1);
        AddRadioButton("", out OpLabel? name, "Selects this profile.");

        ProfileEditor editor = AddProfileEditor(
            name!,
            "Edits this profile's name.",
            "",
            value => RenameProfile(slot, value),
            () => CopyProfile(slot)
        );

        SetColumns(17);

        OpLabel addons = AddNote("Addons enabled: <PLACEHOLDER>", "", alignment: FLabelAlignment.Left, span: 8f)!;
        OpLabel options = AddNote("Options enabled: <PLACEHOLDER>", "", alignment: FLabelAlignment.Right, span: 8f)!;

        AddFoldButton(
            "Shows or hides this profile's details.",
            () => IsProfileExpanded(slot),
            () => ToggleProfile(slot, container)
        );

        BeginFold(() => IsProfileExpanded(slot));

        SetColumns(2);

        OpLabel created = AddNote("Created: <PLACEHOLDER>", "", alignment: FLabelAlignment.Left)!;
        OpLabel modified = AddNote("Modified: <PLACEHOLDER>", "", alignment: FLabelAlignment.Right)!;

        SetColumns(1);

        OpLabel unavailable = AddNote("Unavailable: <PLACEHOLDER> addons and <PLACEHOLDER> options", alignment: FLabelAlignment.Left)!;

        SetColumns(10);
        AddColumn(8f);
        AddHoldButton("Remove Profile", "Hold to remove this profile.", () => RemoveProfile(slot), span: 2f);

        container = EndBox();

        return new(editor, created, modified, addons, options, unavailable);
    }

    private bool IsProfileExpanded(int slot)
    {
        return ProfileAt(slot) is { } profile && _expandedProfiles.Contains(profile.Id);
    }

    private void ToggleProfile(int slot, UIelement? anchor)
    {
        if (ProfileAt(slot) is not { } profile)
        {
            return;
        }

        if (!_expandedProfiles.Remove(profile.Id))
        {
            _expandedProfiles.Add(profile.Id);
        }

        _profileFilter?.Focus(anchor);
    }

    private void SelectProfile(int slot) => SelectProfile(slot, reapplyCurrent: true);

    private void SelectProfile(string id) => SelectProfile(_profiles.FindIndex(profile => profile.Id == id), reapplyCurrent: false);

    private void SelectProfile(int slot, bool reapplyCurrent)
    {
        if (ProfileAt(slot) == null || (!reapplyCurrent && slot == _profileIndex))
        {
            return;
        }

        _profileIndex = slot;

        ApplyProfile();
        RefreshProfileViews();
    }

    private void CreateProfile(string name)
    {
        try
        {
            var created = ProfileManager.Create(name);

            _profiles.Insert(0, created.Profile);
            _profileIndex = 0;
            _profileState = ProfileManager.State();

            ShowProfileResult(created.Result, "Profile created.");
            RefreshProfileViews();
            _profileFilter?.ScrollToTop();
        }
        catch (Exception exception)
        {
            ProfileFailure("Could not create profile.", exception);
        }
    }

    private void ApplyProfile()
    {
        if (CurrentProfile is not { } profile)
        {
            return;
        }

        ShowProfileResult(ProfileManager.Apply(profile), "Profile loaded.");
        _profileState = ProfileManager.State();
    }

    private void SaveCurrentProfileWhenChanged()
    {
        string state = ProfileManager.State();

        if (state == _profileState)
        {
            return;
        }

        _profileState = state;

        if (CurrentProfile is not { } profile)
        {
            return;
        }

        try
        {
            ProfileManager.Update(profile);
            RefreshProfileViews();
        }
        catch (Exception exception)
        {
            ProfileFailure("Could not update profile.", exception);
        }
    }

    private void CopyProfile(int index)
    {
        if (ProfileAt(index) is not { } profile)
        {
            return;
        }

        try
        {
            Profile copy = ProfileManager.Copy(profile, $"{profile.Name} Copy");

            _profiles.Insert(0, copy);
            _profileIndex = 0;

            ApplyProfile();
            RefreshProfileViews();
            _profileFilter?.ScrollToTop();
            SetProfileStatus("Profile copied.");
        }
        catch (Exception exception)
        {
            ProfileFailure("Could not copy profile.", exception);
        }
    }

    private void RenameProfile(int index, string name)
    {
        if (ProfileAt(index) is not { } profile)
        {
            return;
        }

        try
        {
            ProfileManager.Rename(profile, name);

            RefreshProfileViews();
            SetProfileStatus("Profile renamed.");
        }
        catch (Exception exception)
        {
            ProfileFailure("Could not rename profile.", exception);
        }
    }

    private void RemoveProfile(int index)
    {
        if (ProfileAt(index) is not { } profile)
        {
            return;
        }

        try
        {
            bool selected = index == _profileIndex;

            ProfileManager.Remove(profile);
            _profiles.RemoveAt(index);

            if (_profileIndex > index)
            {
                _profileIndex--;
            }
            else if (_profileIndex == index)
            {
                _profileIndex = Math.Min(index, _profiles.Count - 1);
            }

            if (_profileIndex >= _profiles.Count)
            {
                _profileIndex = _profiles.Count - 1;
            }

            if (selected)
            {
                ApplyProfile();
            }

            RefreshProfileViews();
            SetProfileStatus("Profile removed.");
        }
        catch (Exception exception)
        {
            ProfileFailure("Could not remove profile.", exception);
        }
    }

    private void ProfileFailure(string message, Exception exception)
    {
        Reporter.LogError(message, exception);
        SetProfileStatus(message);
    }

    private void SetProfileStatus(string text)
    {
        Menu.Remix.ConfigConnector.ShowAlert(Translate(text));
    }

    private void ShowProfileResult(ProfileResult result, string action)
    {
        SetProfileStatus(Translate("<PLACEHOLDER> <PLACEHOLDER> addons and <PLACEHOLDER> options; <PLACEHOLDER> addons and <PLACEHOLDER> options unavailable.").FillPlaceholders(
            Translate(action),
            result.Addons,
            result.Options,
            result.UnavailableAddons,
            result.UnavailableOptions
        ));
    }

    private Profile? ProfileAt(int slot) => slot >= 0 && slot < _profiles.Count ? _profiles[slot] : null;

    private bool ProfileMatches(int slot)
    {
        if (ProfileAt(slot) is not { } profile)
        {
            return false;
        }

        string query = _profileFilter?.Query?.Trim() ?? "";

        return query.Length == 0 || profile.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void RefreshProfileViews()
    {
        for (int slot = 0; slot < _profileViews.Count; slot++)
        {
            ProfileView view = _profileViews[slot];
            Profile? profile = ProfileAt(slot);

            view.Editor.Refresh(profile?.Name ?? "");
            view.Created.text = Translate("Created: <PLACEHOLDER>").FillPlaceholders(ProfileTime(profile?.CreatedAt));
            view.Modified.text = Translate("Modified: <PLACEHOLDER>").FillPlaceholders(ProfileTime(profile?.ModifiedAt));

            ProfileSummary summary = profile == null ? default : ProfileManager.Summarize(profile);

            view.Addons.text = Translate("Addons enabled: <PLACEHOLDER>").FillPlaceholders($"{summary.EnabledAddons} / {summary.Addons}");
            view.Options.text = Translate("Options enabled: <PLACEHOLDER>").FillPlaceholders($"{summary.EnabledOptions} / {summary.Options}");
            view.Unavailable.text = Translate("Unavailable: <PLACEHOLDER> addons and <PLACEHOLDER> options").FillPlaceholders(
                summary.UnavailableAddons,
                summary.UnavailableOptions
            );
        }

        if (_profileSelector != null)
        {
            RefreshProfileSelector();
        }

        if (_profileRadioGroup != null && _profileIndex >= 0 && _profileRadioGroup.value != _profileIndex.ToString())
        {
            _profileRadioGroup.value = _profileIndex.ToString();
        }

        _profileFilter?.Refresh();
    }

    private void RefreshProfileEditorVisibility()
    {
        _createEditor?.RefreshVisibility(true);

        for (int slot = 0; slot < _profileViews.Count; slot++)
        {
            _profileViews[slot].Editor.RefreshVisibility(ProfileMatches(slot));
        }
    }

    private ListItem[] ProfileChoices(bool includeFallback = false)
    {
        ListItem[] profiles = [.. _profiles.Select((profile, index) => new ListItem(profile.Id, profile.Name, index))];

        return _profiles.Count == 0 || includeFallback
            ? [new ListItem(Config.NoProfile, Translate("No profiles saved")), .. profiles]
            : profiles;
    }

    private void RefreshProfileSelector()
    {
        if (_profileSelector is not { } selector)
        {
            return;
        }

        ListItem[] choices = ProfileChoices();
        string choiceState = _profiles.Count == 0
            ? Config.NoProfile
            : string.Join("\n", _profiles.Select(profile => $"{profile.Id}\0{profile.Name}"));

        if (_profileChoices != choiceState)
        {
            _profileChoices = choiceState;

            string[] existing = [.. selector.GetItemList().Select(item => item.name)];
            ListItem fallback = new(Config.NoProfile, Translate("No profiles saved"));

            if (!existing.Contains(Config.NoProfile))
            {
                selector.AddItems(sort: false, [fallback]);
            }

            selector.RemoveItems(selectNext: false, existing.Where(id => id != Config.NoProfile).ToArray());

            if (_profiles.Count > 0)
            {
                selector.AddItems(sort: false, choices);
                selector.RemoveItems(selectNext: false, [Config.NoProfile]);
            }
        }

        string selected = CurrentProfile?.Id ?? Config.NoProfile;

        if (selector.value != selected)
        {
            selector.value = selected;
        }
    }

    private sealed class ProfileView(ProfileEditor editor, OpLabel created, OpLabel modified, OpLabel addons, OpLabel options, OpLabel unavailable)
    {
        internal ProfileEditor Editor { get; } = editor;

        internal OpLabel Created { get; } = created;

        internal OpLabel Modified { get; } = modified;

        internal OpLabel Addons { get; } = addons;

        internal OpLabel Options { get; } = options;

        internal OpLabel Unavailable { get; } = unavailable;
    }
}
