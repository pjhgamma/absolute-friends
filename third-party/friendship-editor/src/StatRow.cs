using AbsoluteFriends.Manager;
using AbsoluteFriends.Utils;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class StatRow
{
    internal const float LabelWidth = 90f;

    internal const float Height = 34f;

    private const float ModeX = LabelWidth;

    private const float ModeWidth = 150f;

    private const float ValueX = ModeX + ModeWidth + 16f;

    private const float PercentWidth = 44f;

    private readonly OpComboBox _mode;

    private readonly OpSlider _value;

    private readonly OpLabel _percent;

    internal StatRow(FriendTab tab, Stat stat, float y, Action<StatRow> save)
    {
        Stat = stat;
        EditorUi.Label(tab, Translation.Of(stat switch
        {
            Stat.Reputation => "Reputation",
            Stat.Like => "Like",
            Stat.TempLike => "Temp Like",
            _ => "Know",
        }), new Vector2(0f, y), LabelWidth);
        _mode = EditorUi.Combo(tab, EditorUi.ModeItems(false), EditorUi.Inherit, new Vector2(ModeX, y), ModeWidth,
            Translation.Of("Sets the rule for this value."));
        _mode.OnValueUpdate += delegate { save(this); };
        _value = EditorUi.Slider(tab, new Vector2(ValueX, y), tab.Size.x - PercentWidth - 8f - ValueX, stat == Stat.Know ? 0 : -100);
        _percent = EditorUi.Label(tab, string.Empty, new Vector2(tab.Size.x - PercentWidth, y), PercentWidth, FLabelAlignment.Right);
        _value.OnValueUpdate += delegate
        {
            ShowPercent();

            if (IsLimit)
            {
                save(this);
            }
        };
    }

    internal Stat Stat { get; }

    internal OpComboBox Mode => _mode;

    internal Rule? EditedRule => Enum.TryParse(_mode.value, out RuleMode mode) ? new Rule(mode, EditorUi.PercentOf(_value) / 100f) : null;

    private bool IsLimit => EditedRule is { Enforces: true };

    internal void SetModes(bool topLevel) => EditorUi.SetItems(_mode, EditorUi.ModeItems(topLevel), _mode.value);

    internal void Load(Rule? rule)
    {
        string mode = rule?.Mode.ToString() ?? EditorUi.Inherit;

        EditorUi.ShowValue(_mode, _mode.GetItemList().Any(item => item.name == mode) ? mode : EditorUi.Inherit);

        if (rule is { Enforces: true } enforced)
        {
            ShowRuleValue(enforced);
        }
    }

    internal void Refresh(Rule? resolved)
    {
        bool isLimit = IsLimit;
        Rule? inherited = !isLimit && _mode.value == EditorUi.Inherit && resolved is { Enforces: true } broader ? broader : null;

        _value.greyedOut = !isLimit;

        if (inherited is { } rule)
        {
            ShowRuleValue(rule);
        }

        ShowPercent();
    }

    private void ShowRuleValue(Rule rule) => EditorUi.ShowValue(_value, Mathf.RoundToInt(rule.Value * 100f).ToString());

    private void ShowPercent()
    {
        _percent.text = $"{EditorUi.PercentOf(_value)}%";
        _percent.color = _value.greyedOut ? MenuColorEffect.rgbMediumGrey : MenuColorEffect.rgbWhite;
    }
}
