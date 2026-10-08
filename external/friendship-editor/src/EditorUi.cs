using AbsoluteFriends.Manager;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal static class EditorUi
{
    internal const float RowHeight = 24f;

    internal const string Inherit = "Inherit";

    private const float SliderOffset = 3f;

    internal const float ListRowHeight = 30f;

    internal const float ListNameWidth = 220f;

    internal const float ListBarX = 228f;

    internal const float ListBarWidth = 241f;

    internal const float ListBarHeight = 6f;

    internal const float ListMarkOverhang = 2f;

    internal const float ListPercentX = 475f;

    internal const float ListPercentWidth = 40f;

    internal static OpLabel Label(FriendTab tab, string text, Vector2 position, float width, FLabelAlignment alignment = FLabelAlignment.Left, bool grey = false)
    {
        OpLabel label = new(position, new Vector2(width, RowHeight), text, alignment);

        if (grey)
        {
            label.color = MenuColorEffect.rgbMediumGrey;
        }

        tab.Add(label);

        return label;
    }

    internal static ScrollList List(FriendTab tab, string title, float y)
    {
        float height = y - 6f;
        ScrollList list = new(Vector2.zero, new Vector2(tab.Size.x + MenuBuilder.Padding, height));

        tab.Add(new OpLabel(new Vector2(0f, height + 4f), new Vector2(tab.Size.x, 30f), title, FLabelAlignment.Center, true));
        tab.Add(list);

        return list;
    }

    internal static OpComboBox Combo(FriendTab tab, IEnumerable<(string Key, string Name)> items, string selected, Vector2 position, float width, string description)
    {
        List<ListItem> list = [.. items.Select((item, index) => new ListItem(item.Key, item.Name, index))];
        OpComboBox combo = new(new Configurable<string>(list.Any(item => item.name == selected) ? selected : list[0].name), position, width, list)
        {
            description = description
        };

        tab.Add(combo);

        return combo;
    }

    internal static OpSlider Slider(FriendTab tab, Vector2 position, float width, int minimum)
    {
        OpSlider slider = new(new Configurable<int>(0, new ConfigurableInfo(string.Empty, new ConfigAcceptableRange<int>(minimum, 100))), position - new Vector2(0f, SliderOffset), (int)width)
        {
            min = minimum,
            max = 100,
        };

        tab.Add(slider);

        return slider;
    }

    internal static OpImage Pixel(Color color)
    {
        return new OpImage(Vector2.zero, "pixel")
        {
            color = color
        };
    }

    internal static IEnumerable<(string Key, string Name)> ModeItems(bool topLevel) =>
    [
        (Inherit, Translation.Of(topLevel ? "No Rule" : "Inherit")),
        .. ((RuleMode[])[RuleMode.Vanilla, RuleMode.Minimum, RuleMode.Maximum, RuleMode.Fixed])
            .Where(mode => !topLevel || mode != RuleMode.Vanilla)
            .Select(mode => (mode.ToString(), Translation.Of(mode == RuleMode.Vanilla ? "No Rule" : mode.ToString()))),
    ];

    internal static void KeepOpenComboOnTop(IEnumerable<OpComboBox> combos)
    {
        foreach (OpComboBox combo in combos)
        {
            if (combo.held)
            {
                combo.MoveToFront();
            }
        }
    }

    internal static string PercentText(float value) => $"{Mathf.RoundToInt(value * 100f)}%";

    internal static int PercentOf(UIconfig config) => int.TryParse(config.value, out int value) ? value : 0;

    internal static void ShowValue(UIconfig config, string value)
    {
        if (config.value != value)
        {
            config.ForceValue(value);
            config.Change();
        }
    }

    internal static void SetItems(OpComboBox combo, IEnumerable<(string Key, string Name)> items, string selected)
    {
        ListItem[] list = [.. items.Select((item, index) => new ListItem(item.Key, item.Name, index))];

        combo._itemList = list;

        if (list.All(item => item.name != selected))
        {
            selected = list[0].name;
        }

        combo.ForceValue(selected);
        combo._ResetIndex();
        combo.Change();
        combo._lblText.text = LabelTest.TrimText(combo._GetDisplayValue(), combo.size.x - 32f, true);
    }

    internal static void SetShown(UIelement element, bool shown)
    {
        if (shown && element.Hidden)
        {
            element.Show();
            Snap(element);
        }
        else if (!shown && !element.Hidden)
        {
            element.Hide();
        }
    }

    internal static void Place(UIelement element, Vector2 position)
    {
        if (element.GetPos() != position)
        {
            element.SetPos(position);
            Snap(element);
        }
    }

    internal static void FitIcon(OpImage icon)
    {
        Vector2 size = icon.sprite.element.sourcePixelSize;
        float scale = Mathf.Min(1f, 24f / Mathf.Max(size.x, size.y));

        icon.scale = new Vector2(scale, scale);
    }

    internal static string Trim(string text, float width) => LabelTest.GetWidth(text) > width ? LabelTest.TrimText(text, width, true) : text;

    private static void Snap(UIelement element)
    {
        element.lastScreenPos = element.ScreenPos;
        element.GrafUpdate(1f);
    }
}
