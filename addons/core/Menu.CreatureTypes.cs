using System.Reflection;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Core;

internal static partial class Menu
{
    private const ushort VisibleRuleItems = 3;

    private static readonly FieldInfo? _listTopField = typeof(OpComboBox).GetField("_listTop", BindingFlags.Instance | BindingFlags.NonPublic);

    private static MenuBuilder? _resetMenu;

    private static OnEventHandler? _resetHandler;

    private static void BuildCreatureTypes(MenuBuilder menu)
    {
        CreatureRules.Refresh();

        ListItem[] rules =
        [
            RuleItem(CreatureRule.Allow),
            RuleItem(CreatureRule.AllowNeutral),
            RuleItem(CreatureRule.Deny),
        ];
        ListItem[] types = [.. CreatureRules.Options.Select(type => new ListItem(type.Key))];

        if (types.Length > 0 && !types.Any(item => item.name == Config.SelectedCreatureType.Value))
        {
            Config.SelectedCreatureType.Value = types[0].name;
        }

        if (!rules.Any(item => item.name == Config.SelectedCreatureRule.Value))
        {
            Config.SelectedCreatureRule.Value = nameof(CreatureRule.Allow);
        }

        OpComboBox? typePicker = null;
        OpComboBox? rulePicker = null;
        OpListBox? configuredRules = null;

        void ShowType(string type)
        {
            if (rulePicker == null)
            {
                return;
            }

            CreatureRule rule = CreatureRules.Get(type);
            string value = (rule == CreatureRule.Inherit ? CreatureRule.Allow : rule).ToString();

            if (rulePicker.value != value)
            {
                rulePicker.value = value;
            }

            if (rule != CreatureRule.Inherit && configuredRules is { } list
                && list.value != type && list.GetItemList().Any(item => item.name == type))
            {
                list.value = type;
            }
        }

        void ShowConfiguredRule(string type)
        {
            if (CreatureRules.Get(type) == CreatureRule.Inherit)
            {
                if (configuredRules is { } list && list.GetItemList() is { Length: > 0 } items && list.value != items[0].name)
                {
                    list.value = items[0].name;
                }

                return;
            }

            if (typePicker == null)
            {
                return;
            }

            if (typePicker.value != type)
            {
                typePicker.value = type;
            }

            ShowType(type);
        }

        void RefreshConfiguredRules(string? selection = null, bool reset = false)
        {
            if (configuredRules == null)
            {
                return;
            }

            selection ??= configuredRules.value;

            string[] previous = [.. configuredRules.GetItemList().Select(item => item.name)];
            ListItem[] current = ConfiguredRuleItems(reset);

            _listTopField?.SetValue(configuredRules, 0);
            configuredRules.AddItems(sort: false, [new ListItem("__refresh", " ")]);
            configuredRules.RemoveItems(selectNext: false, previous);
            configuredRules.AddItems(sort: false, current);
            configuredRules.RemoveItems(selectNext: false, ["__refresh"]);
            configuredRules.value = current.Any(item => item.name == selection && CreatureRules.Get(item.name) != CreatureRule.Inherit)
                ? selection
                : current[0].name;
        }

        CreatureRules.HandleChanged = () => RefreshConfiguredRules();

        if (_resetMenu != null && _resetHandler != null)
        {
            _resetMenu.OnConfigReset -= _resetHandler;
        }

        _resetMenu = menu;
        _resetHandler = () =>
        {
            RefreshConfiguredRules(reset: true);

            rulePicker?.value = nameof(CreatureRule.Allow);
        };
        menu.OnConfigReset += _resetHandler;

        ListItem[] initialRules = ConfiguredRuleItems();

        if (CreatureRules.Get(Config.SelectedConfiguredCreatureRules.Value) == CreatureRule.Inherit
            || !initialRules.Any(item => item.name == Config.SelectedConfiguredCreatureRules.Value))
        {
            Config.SelectedConfiguredCreatureRules.Value = initialRules[0].name;
        }

        menu.AddParallelColumns(
            () =>
            {
                menu.AddLabel("Creature Type", alignment: FLabelAlignment.Left);
                typePicker = menu.AddComboBox(Config.SelectedCreatureType, types);
                menu.AddLabel("Creature Rule", alignment: FLabelAlignment.Left);
                rulePicker = menu.AddComboBox(Config.SelectedCreatureRule, rules, static (option, position, width, items) => new RuleComboBox(option, position, width, items));
            },
            () =>
            {
                configuredRules = menu.AddListBox(Config.SelectedConfiguredCreatureRules, initialRules, static (option, position, width, items, count) => new ConfiguredRuleListBox(option, position, width, items, count), "Configured Creature Rules", visibleItems: VisibleRuleItems);
            }
        );

        typePicker?.OnValueUpdate += (_, value, _) => ShowType(value);
        configuredRules?.OnValueUpdate += (_, value, _) => ShowConfiguredRule(value);

        if (configuredRules != null && CreatureRules.Get(configuredRules.value) != CreatureRule.Inherit)
        {
            ShowConfiguredRule(configuredRules.value);
        }
        else
        {
            ShowType(typePicker?.value ?? "");
        }

        menu.AddRow(0.5f);
        menu.SetColumns(2);
        menu.AddSimpleButton("Apply Creature Rule", "Add or update the selected creature rule.", () =>
        {
            string type = typePicker?.value ?? "";

            CreatureRules.Set(type, rulePicker?.value ?? "");
            RefreshConfiguredRules(type);
        });
        menu.AddSimpleButton("Remove Creature Rule", "Remove the selected creature rule and use the global friendship options again.", () =>
        {
            if (configuredRules is { } list && CreatureRules.Get(list.value) != CreatureRule.Inherit)
            {
                CreatureRules.Set(list.value, nameof(CreatureRule.Inherit));
            }
        });
    }

    private static ListItem RuleItem(CreatureRule rule)
    {
        return new(rule.ToString(), Translation.Of(RuleLabel(rule)));
    }

    private static string RuleLabel(CreatureRule rule) => rule switch
    {
        CreatureRule.Allow => "Allow friendly",
        CreatureRule.AllowNeutral => "Allow friendly & neutral",
        CreatureRule.Deny => "Deny friendship",
        _ => ""
    };

    private static ListItem[] ConfiguredRuleItems(bool reset = false)
    {
        List<ListItem> items = [];

        if (!reset)
        {
            foreach (var pair in CreatureRules.ConfiguredRules)
            {
                items.Add(new(pair.Key));
            }
        }

        if (items.Count == 0)
        {
            items.Add(new(Config.NoConfiguredRule, Translation.Of("No creature rules")));
        }

        for (int index = items.Count; index < VisibleRuleItems; index++)
        {
            items.Add(new($"__none{index}", " ", int.MaxValue));
        }

        return [.. items];
    }

    private sealed class RuleComboBox(Configurable<string> option, Vector2 position, float width, List<ListItem> items)
        : OpComboBox(option, position, width, items)
    {
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            RuleColors.Apply(_lblText, _lblList, _searchMode ? _searchList : _itemList, _listTop, value, CreatureRules.Parse);
        }
    }

    private sealed class ConfiguredRuleListBox(Configurable<string> option, Vector2 position, float width, List<ListItem> items, ushort visibleItems)
        : OpListBox(option, position, width, items, visibleItems, true)
    {
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            RuleColors.Apply(_lblText, _lblList, _searchMode ? _searchList : _itemList, _listTop, value, CreatureRules.Get);
        }
    }

    private static class RuleColors
    {
        internal static void Apply(FLabel? selected, FLabel[]? labels, IReadOnlyList<ListItem>? items, int top, string value, Func<string, CreatureRule> ruleFor)
        {
            if (selected != null && Get(ruleFor(value)) is { } color)
            {
                selected.color = color;
            }

            if (labels == null || items == null)
            {
                return;
            }

            for (int index = 0; index < labels.Length; index++)
            {
                int itemIndex = top + index;

                if (itemIndex >= 0 && itemIndex < items.Count && labels[index] is { } label && Get(ruleFor(items[itemIndex].name)) is { } color2)
                {
                    label.color = color2;
                }
            }
        }

        private static Color? Get(CreatureRule rule) => rule switch
        {
            CreatureRule.Allow => Color.green,
            CreatureRule.AllowNeutral => Color.white,
            CreatureRule.Deny => Color.red,
            _ => null
        };
    }
}
