using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace AbsoluteFriends.Options;

public abstract partial class MenuBuilder
{
    protected void BindRequirements()
    {
        foreach (var option in _options)
        {
            Configurable<bool>[][] groups = [.. option.Requirements];

            if (groups.Length > 0)
            {
                BindMasters(option, groups);
            }
        }
    }

    private void Register(ConfigurableBase configurableBase, UIfocusable focusable, OpLabel? label = null)
    {
        if (_entries.ContainsKey(configurableBase))
        {
            return;
        }

        _options.Add(configurableBase);
        _entries[configurableBase] = new(focusable, label, focusable.description);
    }

    private void Mark(OptionEntry entry, Color color, string description, bool failed = false)
    {
        if (entry.Focusable is OpCheckBox checkBox)
        {
            checkBox.colorEdge = color;
        }

        if (entry.Label is { } label)
        {
            label.color = color;
        }

        entry.SetMark(description, failed);
    }

    private void Propagate(ConfigurableBase configurableBase)
    {
        if (_propagates.TryGetValue(configurableBase, out Action propagate))
        {
            propagate?.Invoke();
        }
    }

    private bool IsMasterActive(Configurable<bool> master)
    {
        return _entries.TryGetValue(master, out OptionEntry entry)
            ? !entry.Focusable.greyedOut && (entry.Focusable is not OpCheckBox checkBox || checkBox.GetValueBool())
            : master.IsActive;
    }

    private string FormatRequirement(Configurable<bool>[] group)
    {
        string[] labels = [.. group.Select(master => Translate(master.Label ?? "")).Distinct()];
        string listed = string.Join(", ", labels);

        return labels.Length > 1 ? Translate("one of <PLACEHOLDER>").FillPlaceholders(listed) : listed;
    }

    private void BindMasters(ConfigurableBase target, Configurable<bool>[][] groups)
    {
        if (groups.Length == 0 || !_entries.TryGetValue(target, out OptionEntry entry))
        {
            return;
        }

        void UpdateTarget()
        {
            string[] missing = [.. groups.Where(group => group.All(master => !IsMasterActive(master))).Select(FormatRequirement)];
            bool greyedOut = missing.Length > 0;

            entry.SetRequirement(greyedOut
                ? Translate("Turn on <PLACEHOLDER> first to use this option.").FillPlaceholders(string.Join(", ", missing))
                : "");

            greyedOut |= entry.IsFailed;

            if (entry.Focusable.greyedOut != greyedOut)
            {
                entry.SetGreyedOut(greyedOut);
                Propagate(target);
            }
        }

        bool isExternal = false;

        foreach (var master in groups.SelectMany(group => group))
        {
            if (_entries.TryGetValue(master, out OptionEntry masterEntry))
            {
                masterEntry.Focusable.OnChange += UpdateTarget;
                _propagates[master] = _propagates.TryGetValue(master, out Action propagate) ? propagate + UpdateTarget : UpdateTarget;
            }
            else
            {
                isExternal = true;
            }
        }

        if (isExternal)
        {
            _externals.Add(UpdateTarget);
        }

        UpdateTarget();
    }
}
