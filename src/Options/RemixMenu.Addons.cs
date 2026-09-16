using Menu.Remix.MixedUI;
using RippleFriends.Addons;
using RippleFriends.Utils;

namespace RippleFriends.Options;

internal sealed partial class RemixMenu
{
    private void BuildAddonGroups(OpTab tab)
    {
        BeginTab(tab);
        AddTitle("Addons");
        AddLabel("Select a profile, then enable addons and configure their options.");

        SetColumns(2);
        _profileSelector = AddComboBox(Config.ProfileSelection, ProfileChoices(includeFallback: true), "Selected Profile", span: 2f);

        if (_profileSelector != null)
        {
            _profileSelector.OnValueUpdate += (_, value, _) => SelectProfile(value);
            Watch(() =>
            {
                if (_profileSelector.held)
                {
                    _profileSelector.MoveToFront();
                }
            });
            RefreshProfileSelector();
        }

        SectionFilter? filter = AddListSearch("_searchAddons", "Shows only the addons whose name or options contain what is typed here.");
        Addon[] addons = [.. AddonRegistry.Ordered.Where(addon => !addon.IsApplied || addon.HasMenu)];

        Watch(() => Search(filter, addons));

        BuildAddonGroup("Built-In Addons", [.. addons.Where(addon => addon.IsBuiltIn)], filter);
        BuildAddonGroup("Third-Party Addons", [.. addons.Where(addon => !addon.IsBuiltIn)], filter);
    }

    private void BuildAddonGroup(string title, Addon[] addons, SectionFilter? filter)
    {
        if (addons.Length == 0)
        {
            return;
        }

        BeginSection([.. addons.SelectMany(addon => addon.Keywords)]);
        AddTitle(title);

        foreach (var addon in addons)
        {
            BuildAddon(addon, filter);
        }
    }

    private void BuildAddon(Addon addon, SectionFilter? filter)
    {
        OpRect? container = null;

        BeginSection([.. addon.Keywords]);
        BeginBox();

        SetColumns(2);
        if (addon.Enabled is { } enabled)
        {
            AddCheckBox(enabled, enabled: addon.AddonDependenciesApplied);
        }
        else
        {
            AddNote(addon.Name, alignment: FLabelAlignment.Left);
        }
        AddNote($"{addon.Id} v{addon.Version}");

        SetColumns(1);

        if (!addon.IsApplied && AddLabel("Remix is not enabled.", alignment: FLabelAlignment.Left) is { } notApplied)
        {
            notApplied.color = Palette.Secondary;
        }

        if (addon.AddonDependencyIds.Count > 0)
        {
            AddLabel("Requires: <PLACEHOLDER>", string.Join(", ", addon.AddonDependencyIds.Select(AddonRegistry.NameOf)), alignment: FLabelAlignment.Left);
        }

        if (addon.IsMismatched && AddLabel("Built for Ripple Friends v<PLACEHOLDER>; it may not work correctly.", addon.RippleFriendsVersion, alignment: FLabelAlignment.Left) is { } warning)
        {
            warning.color = Palette.Secondary;
        }

        if (!addon.IsApplied)
        {
            EndBox();

            return;
        }

        SetColumns(16);
        AddColumn(8f);

        OpLabel? options = AddNote("Options enabled: <PLACEHOLDER>", OptionCountText(addon), span: 7f);

        AddFoldButton(
            "Shows or hides this addon's options.",
            () => IsExpanded(addon),
            () => Expand(addon, filter, container),
            span: 1f
        );

        if (options != null)
        {
            string template = Translate("Options enabled: <PLACEHOLDER>");

            Watch(() => Retitle(options, template, addon));
        }

        BeginFold(() => IsExpanded(addon));

        SetColumns(4);
        AddonRegistry.BuildMenu(addon, this);

        container = EndBox();
    }

    private bool IsExpanded(Addon addon)
    {
        return _expanded.Contains(addon) || _searched.Contains(addon);
    }

    private void Expand(Addon addon, SectionFilter? filter, UIelement? anchor)
    {
        if (_searched.Remove(addon) | _expanded.Remove(addon))
        {
            return;
        }

        _expanded.Add(addon);

        filter?.Focus(anchor);
    }

    private void Search(SectionFilter? filter, Addon[] addons)
    {
        if (filter?.Query is not { } query || query == _query)
        {
            return;
        }

        _query = query;

        _searched.Clear();

        if (query.Length == 0)
        {
            return;
        }

        foreach (var addon in addons.Where(addon => addon.HasOption(query)))
        {
            _searched.Add(addon);
        }
    }
}
