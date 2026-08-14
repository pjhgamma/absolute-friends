using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RippleFriends.Core;
using RippleFriends.Diagnostics;
using UnityEngine;

namespace RippleFriends.Options;

internal abstract class RemixMenuBuilder : OptionInterface
{
    private const float _padding = 30f;

    private const float _spacing = 25f;

    private const float _gap = 5f;

    private int _columns = 4;

    private float _currentColumn = 0f;

    private readonly Vector2 MarginX = new(_padding, 600f - _padding);

    private float Width => MarginX.y - MarginX.x;

    private float ElementWidth => Width / _columns;

    private Vector2 _pos = new();

    private OpTab? _currentTab;

    private readonly List<OptionCheckBox> _optionCheckBoxes = [];

    private readonly struct OptionCheckBox(OpCheckBox checkBox, OpLabel label, Configurable<bool> option)
    {
        public readonly OpCheckBox CheckBox = checkBox;

        public readonly OpLabel Label = label;

        public readonly Configurable<bool> Option = option;
    }

    private static void Mark(OptionCheckBox entry, Color color, string description)
    {
        entry.CheckBox.colorEdge = color;
        entry.CheckBox.description += "\n" + description;
        entry.Label.color = color;
        entry.Label.description += "\n" + description;
    }

    protected void SetCurrentTab(OpTab? tab)
    {
        _currentTab = tab;
        _pos = new(MarginX.x, 600f);
        _currentColumn = 0;

        AddNewLine();
    }

    protected void AddNewLine(float spacingModifier = 1f)
    {
        _pos.x = MarginX.x;
        _pos.y -= spacingModifier * _spacing;
        _currentColumn = 0;
    }

    protected void ResetColumn()
    {
        if (_currentColumn > 0)
        {
            AddNewLine(1.5f);
        }
    }

    protected void SetColumns(int columns)
    {
        ResetColumn();

        _columns = columns;
    }

    protected void AddLabel(string text, FLabelAlignment alignment = FLabelAlignment.Left, bool bigText = false)
    {
        if (_currentTab == null)
        {
            return;
        }

        ResetColumn();

        float height = (bigText ? 1.5f : 1f) * _spacing;

        if (bigText)
        {
            AddNewLine(0.5f);
        }

        OpLabel label = new(
            new(_pos.x, _pos.y - 6f),
            new(Width, height),
            Translate(text),
            alignment,
            bigText
        )
        {
            autoWrap = true,
        };

        _currentTab.AddItems(label);
        AddNewLine();
    }

    protected void AddTitle(string? title = null, string? description = null, FLabelAlignment alignment = FLabelAlignment.Center)
    {
        ResetColumn();
        AddNewLine();

        if (title != null)
        {
            AddLabel(title, alignment, true);
        }
        if (description != null)
        {
            AddLabel(description, alignment);
        }
    }

    protected OpCheckBox? AddCheckBox(Configurable<bool> configurable, string? text = null, OpCheckBox? master = null)
    {
        if (_currentTab == null)
        {
            return null;
        }

        text ??= configurable.Label();

        string desc = Translate(configurable.info?.description ?? "");

        OpLabel label = new(
            new Vector2(_pos.x, _pos.y - _spacing * 0.5f),
            new(ElementWidth - _spacing - _gap, _spacing),
            Translate(text ?? ""),
            FLabelAlignment.Right
        )
        {
            description = desc
        };
        OpCheckBox checkBox = new(
            configurable,
            new Vector2(_pos.x + ElementWidth - _spacing, _pos.y - _spacing * 0.5f)
        )
        {
            description = desc
        };

        if (master != null)
        {
            checkBox.greyedOut = !master.GetValueBool();
            master.OnChange += delegate
            {
                checkBox.greyedOut = !master.GetValueBool();
            };
        }

        _optionCheckBoxes.Add(new(checkBox, label, configurable));

        if (text != null)
        {
            _currentTab.AddItems(label);
        }
        _currentTab.AddItems(checkBox);

        _pos.x += ElementWidth;
        if ((_currentColumn += 1) > _columns - 1)
        {
            AddNewLine(1.5f);
        }

        return checkBox;
    }

    protected void AddFloatSlider(Configurable<float> configurable, float span = 1, float min = 0f, float max = 5f, string? text = null, OpCheckBox? master = null)
    {
        if (_currentTab == null)
        {
            return;
        }

        if (_currentColumn > _columns - span + 0.5f)
        {
            ResetColumn();
        }

        text ??= configurable.Label();

        string desc = Translate(configurable.info?.description ?? "");

        OpLabel label = new(
            new Vector2(_pos.x, _pos.y - _spacing * 0.5f),
            new(ElementWidth - _spacing - _gap, _spacing),
            Translate(text ?? ""),
            FLabelAlignment.Right
        )
        {
            description = desc
        };
        OpFloatSlider slider = new(
            configurable,
            new Vector2(_pos.x + _gap + (text != null ? ElementWidth : 0), _pos.y - _spacing * 0.5f - 3f),
            (int)(ElementWidth * span - _gap * 2f - (text != null ? ElementWidth - _spacing : 0))
        )
        {
            description = desc,
            min = min,
            max = max,
        };

        if (master != null)
        {
            slider.greyedOut = !master.GetValueBool();
            master.OnChange += delegate
            {
                slider.greyedOut = !master.GetValueBool();
            };
        }

        if (text != null)
        {
            _currentTab.AddItems(label);
        }
        _currentTab.AddItems(slider);

        _pos.x += ElementWidth * span;
        if ((_currentColumn += span) > _columns - 1)
        {
            AddNewLine(1.5f);
        }
    }

    protected OpSimpleButton? AddSimpleButton(string text, Action action, float span = 1f, string? description = null, OpCheckBox? master = null)
    {
        if (_currentTab == null)
        {
            return null;
        }

        if (_currentColumn > _columns - span + 0.5f)
        {
            ResetColumn();
        }

        OpSimpleButton simpleButton = new(
            new Vector2(_pos.x + _gap, _pos.y - _spacing * 0.5f),
            new(ElementWidth * span - _gap * 2f, _spacing),
            Translate(text)
        )
        {
            description = Translate(description ?? "")
        };

        simpleButton.OnClick += delegate
        {
            action();
        };

        if (master != null)
        {
            simpleButton.greyedOut = !master.GetValueBool();
            master.OnChange += delegate
            {
                simpleButton.greyedOut = !master.GetValueBool();
            };
        }

        _currentTab.AddItems(simpleButton);

        _pos.x += ElementWidth * span;
        if ((_currentColumn += span) > _columns - 1)
        {
            AddNewLine(1.5f);
        }

        return simpleButton;
    }

    public override void Update()
    {
        base.Update();

        foreach (var entry in _optionCheckBoxes)
        {
            if (Hooks.HookManager.IsFailed(entry.Option))
            {
                if (entry.CheckBox.greyedOut)
                {
                    continue;
                }

                entry.CheckBox.greyedOut = true;
                entry.CheckBox.symbolSprite.isVisible = false;

                Mark(entry, Palette.Primary, Translate("This feature ran into an error and disabled for this session. Restart the game to try it again."));
            }
            else if (!entry.CheckBox.greyedOut && Hooks.HookManager.IsWarned(entry.Option) && entry.Label.color != Palette.Secondary)
            {
                Mark(entry, Palette.Secondary, Translate("This feature was applied, but not the way it expected, so it may not be working."));
            }
        }
    }
}

internal class RemixMenu : RemixMenuBuilder
{
    public static readonly RemixMenu Instance = new();

    public RemixMenu()
    {
        Config.Bind(this);
    }

    public override void Initialize()
    {
        base.Initialize();

        List<OpTab> enabledTabs = [];
        OpTab generalTab = new(this, Translate("General"));
        OpTab vanillaTab = new(this, Translate("Vanilla"));
        OpTab? downpourTab = null;
        OpTab? watcherTab = null;
        OpTab diagnosticsTab = new(this, Translate("Diagnostics"));

        enabledTabs.Add(generalTab);
        enabledTabs.Add(vanillaTab);
        if (ModManager.MSC)
        {
            downpourTab = new(this, Translate("Downpour"));
            enabledTabs.Add(downpourTab);
        }
        if (ModManager.Watcher)
        {
            watcherTab = new(this, Translate("Watcher"));
            enabledTabs.Add(watcherTab);
        }
        enabledTabs.Add(diagnosticsTab);
        Tabs = [.. enabledTabs];

        SetColumns(4);

        SetCurrentTab(generalTab);

        AddLabel("Ripple Friends", FLabelAlignment.Center, bigText: true);
        AddLabel("Ripple friends do not affect each other.");
        AddLabel("This option itself does nothing, but targets to be affected by the other options.");
        AddLabel("The Ripple Friends relationship applies bidirectionally, excluding oneself.");
        AddCheckBox(Config.FriendPlayer);
        AddCheckBox(Config.FriendCreature);
        AddCheckBox(Config.FriendNeutralCreature);
        AddCheckBox(Config.FriendIterator);
        AddCheckBox(Config.FriendGrabbed);
        AddCheckBox(Config.FriendArena);

        AddTitle("General");
        AddCheckBox(Config.Collision);
        AddCheckBox(Config.Weapon);
        AddCheckBox(Config.Explosion);

        SetCurrentTab(vanillaTab);

        AddTitle("Player Actions");
        var GrabPlayerCheckBox = AddCheckBox(Config.GrabPlayer);
        AddFloatSlider(Config.GrabPlayerTime, span: 3f, master: GrabPlayerCheckBox);
        AddCheckBox(Config.NoStealing);
        AddCheckBox(Config.Pebbles);
        AddCheckBox(Config.Moon);
        AddCheckBox(Config.Mushroom);

        AddTitle("Interactions");
        AddCheckBox(Config.FirecrackerPlant);
        AddCheckBox(Config.Bee);
        AddCheckBox(Config.JellyFish);
        AddCheckBox(Config.Snail);
        AddCheckBox(Config.TubeWorm);

        if (ModManager.MSC)
        {
            SetCurrentTab(downpourTab);

            AddTitle("Player Actions");
            AddCheckBox(Config.GourmandSlam);
            AddCheckBox(Config.ArtificerParry);
            AddCheckBox(Config.SaintTongue);
            AddCheckBox(Config.SaintAttunement);

            AddTitle("Interactions");
            AddCheckBox(Config.FireEgg);
            AddCheckBox(Config.SingularityBomb);
        }

        if (ModManager.Watcher)
        {
            SetCurrentTab(watcherTab);

            AddTitle("Interactions");
            AddCheckBox(Config.Pomegranate);
            AddCheckBox(Config.Frog);
        }

        SetCurrentTab(diagnosticsTab);

        AddLabel("Diagnostics", FLabelAlignment.Center, bigText: true);
        AddLabel("If a feature breaks, it turns itself off and marks its option accordingly.");
        AddLabel("These options only add the detail a bug report needs:");
        AddLabel("turn them on and reproduce the problem, then copy the report.");

        AddTitle("Visualizer");
        AddCheckBox(Config.FriendLink);
        AddCheckBox(Config.OwnerLink);
        AddCheckBox(Config.OwnerName);

        AddTitle("Report");
        var debug = AddCheckBox(Config.Debug);
        AddSimpleButton("Copy Report", HookDiagnostics.CopyReport, span: 2f, description: "Copies the report to the clipboard and saves it in the game folder.", master: debug);
    }
}
