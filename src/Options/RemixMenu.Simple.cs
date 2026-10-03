using AbsoluteFriends.Addons;
using Menu.Remix.MixedUI;

namespace AbsoluteFriends.Options;

internal sealed partial class RemixMenu
{
    private static SwitchState StateOf(Addon addon, Configurable<bool>[] options)
    {
        int count = options.Count(option => option.IsChecked);

        return count == 0 ? SwitchState.Off : count == options.Length ? SwitchState.On : SwitchState.Mixed;
    }

    private static void Toggle(Addon addon, Configurable<bool>[] options)
    {
        bool value = StateOf(addon, options) != SwitchState.On;

        foreach (var option in options)
        {
            option.Check(value);
        }
    }

    private static bool IsAllowed(Addon addon, Configurable<bool>[] options, Configurable<bool> option)
    {
        return option.Requirements.All(masters => masters.Any(master => IsSatisfied(addon, options, master)));
    }

    private static bool IsSatisfied(Addon addon, Configurable<bool>[] options, Configurable<bool> master)
    {
        return options.Contains(master) || (master.IsChecked && IsAllowed(addon, options, master));
    }

    private bool IsAvailable(Addon addon, Configurable<bool>[] options)
    {
        return (addon.Enabled == null || (IsUsable(addon.Enabled) && addon.Enabled.IsChecked)) && options.Any(option => IsAllowed(addon, options, option));
    }

    private void BuildSimpleAddon(Addon addon)
    {
        int switches = 0;

        BeginSection([.. addon.Keywords, .. addon.Groups.Select(group => group.Label)], () => !IsDetailed);
        BeginBox();

        SetColumns(3);
        if (addon.Enabled is { } enabled)
        {
            AddSwitch(
                addon.Name,
                addon.AddonDescription,
                () => enabled.IsChecked ? SwitchState.On : SwitchState.Off,
                () => IsUsable(enabled),
                () => enabled.Check(!enabled.IsChecked),
                span: 2f
            );
        }
        else
        {
            AddNote(addon.Name, alignment: FLabelAlignment.Left, span: 2f);
        }

        if (AddNote("Options enabled: <PLACEHOLDER>", OptionCountText(addon)) is { } count)
        {
            string template = Translate("Options enabled: <PLACEHOLDER>");

            Watch(() => Retitle(count, template, addon));
        }

        foreach (var group in addon.Groups)
        {
            Configurable<bool>[] options = [.. group.Options.Where(HasControl)];

            if (options.Length == 0)
            {
                continue;
            }

            AddSwitch(group.Label, group.Description, () => StateOf(addon, options), () => IsAvailable(addon, options), () => Toggle(addon, options));
            switches++;
        }

        if (switches == 0 && addon.HasUngroupedOptions)
        {
            SetColumns(1);
            AddNote("Options are in the Detailed view.", alignment: FLabelAlignment.Left);
        }

        EndBox();
    }
}
