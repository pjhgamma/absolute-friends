using System.Reflection;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class RuleList
{
    private const ushort VisibleItems = 3;

    private const float ListHeight = VisibleItems * 20f + 34f;

    private const float Gap = 6f;

    private const float NextGap = 40f;

    internal const float Height = Gap + ListHeight + Gap + EditorUi.RowHeight + NextGap;

    private const string Empty = "__empty";

    private static readonly FieldInfo? _listTopField = typeof(OpComboBox).GetField("_listTop", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly OpListBox _list;

    private readonly OpSimpleButton _remove;

    private readonly List<RuleKey> _keys = [];

    private string _shown = string.Empty;

    internal RuleList(EditorView view, float y, Action apply, Action<RuleKey> remove, Action<RuleKey> select)
    {
        float width = view.Tab.Size.x;
        OpSimpleButton applyButton = new(new Vector2(0f, y), new Vector2(width, EditorUi.RowHeight), Translation.Of("Apply Rule"))
        {
            description = Translation.Of("Add or update the rules shown above.")
        };

        applyButton.OnClick += _ => apply();
        view.Add(applyButton);
        y -= Gap + ListHeight;
        _list = new OpListBox(new Configurable<string>(Empty), new Vector2(0f, y), width, [.. Items([]).Select((item, index) => new ListItem(item.Key, item.Name, index))], VisibleItems)
        {
            description = Translation.Of("Choose a rule to show it above.")
        };
        _list.OnValueUpdate += delegate
        {
            if (Selected is { } key)
            {
                select(key);
            }
        };
        view.Add(_list);
        y -= Gap + EditorUi.RowHeight;
        _remove = new OpSimpleButton(new Vector2(0f, y), new Vector2(width, EditorUi.RowHeight), Translation.Of("Remove Rule"))
        {
            description = Translation.Of("Remove the selected rule.")
        };
        _remove.OnClick += _ =>
        {
            if (Selected is { } key)
            {
                remove(key);
            }
        };
        view.Add(_remove);
    }

    private RuleKey? Selected => int.TryParse(_list.value, out int index) && index >= 0 && index < _keys.Count ? _keys[index] : null;

    internal RuleKey? Choose(IReadOnlyCollection<RuleKey> keys, IEnumerable<RuleKey> current)
    {
        foreach (RuleKey key in current)
        {
            if (keys.Contains(key))
            {
                return key;
            }
        }

        if (Selected is { } previous && keys.Contains(previous))
        {
            return previous;
        }

        return keys.Count > 0 ? keys.First() : null;
    }

    internal void Show(IEnumerable<(RuleKey Key, string Text)> rules, RuleKey? selection = null)
    {
        List<(RuleKey Key, string Text)> list = [.. rules];
        string shown = string.Join("\n", list.Select(rule => rule.Text));
        RuleKey? selected = selection ?? Selected;

        _remove.greyedOut = list.Count == 0;

        if (shown == _shown && selection == null)
        {
            return;
        }

        _shown = shown;
        _keys.Clear();
        _keys.AddRange(list.Select(rule => rule.Key));

        int index = selected is { } key ? _keys.IndexOf(key) : -1;
        List<(string Key, string Name)> items = Items(list.Select(rule => rule.Text));

        EditorUi.SetItems(_list, items, items[Math.Max(index, 0)].Key);

        if (_listTopField?.GetValue(_list) is int top && top > Math.Max(0, items.Count - VisibleItems))
        {
            _listTopField.SetValue(_list, 0);
        }
    }

    private static List<(string Key, string Name)> Items(IEnumerable<string> texts)
    {
        List<(string Key, string Name)> items = [.. texts.Select((text, index) => (index.ToString(), text))];

        if (items.Count == 0)
        {
            items.Add((Empty, Translation.Of("No rules")));
        }

        for (int index = items.Count; index < VisibleItems; index++)
        {
            items.Add(($"__none{index}", " "));
        }

        return items;
    }
}
