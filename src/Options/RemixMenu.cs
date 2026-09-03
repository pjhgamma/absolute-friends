using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RippleFriends.Diagnostics;
using RippleFriends.Hooks;
using RippleFriends.Utils;
using UnityEngine;

namespace RippleFriends.Options;

internal abstract class RemixMenuBuilder : OptionInterface
{
    private const float CanvasSize = 600f;

    private const float Padding = 30f;

    private const float Spacing = 25f;

    private const float Gap = 5f;

    private static readonly Vector2 _marginX = new(Padding, CanvasSize - Padding);

    private readonly List<UIelement> _elements = [];

    private readonly List<ConfigurableBase> _options = [];

    private readonly Dictionary<ConfigurableBase, OptionEntry> _entries = [];

    private readonly Dictionary<ConfigurableBase, Action> _propagates = [];

    private OpTab? _currentTab;

    private Vector2 _pos = new();

    private int _columns = 4;

    private float _currentColumn = 0f;

    private static float Width => _marginX.y - _marginX.x;

    private float ElementWidth => Width / _columns;

    public override void Initialize()
    {
        base.Initialize();

        _currentTab = null;

        _elements.Clear();
        _options.Clear();
        _entries.Clear();
        _propagates.Clear();
    }

    public override void Update()
    {
        base.Update();

        foreach (var option in _options)
        {
            if (option is not Configurable<bool> boolOption || !_entries.TryGetValue(option, out OptionEntry entry) || entry.Focusable is not OpCheckBox checkBox)
            {
                continue;
            }

            if (HookManager.IsFailed(boolOption))
            {
                checkBox.greyedOut = true;
                checkBox.symbolSprite.isVisible = false;

                if (checkBox.colorEdge != Palette.Primary)
                {
                    Mark(entry, Palette.Primary, Translate("This feature ran into an error and disabled for this session. Restart the game to try it again."), failed: true);
                    Propagate(option);
                }
            }
            else if (HookManager.IsWarned(boolOption) && checkBox.colorEdge != Palette.Primary && checkBox.colorEdge != Palette.Secondary)
            {
                Mark(entry, Palette.Secondary, Translate("This feature was applied, but not the way it expected, so it may not be working."));
            }
        }
    }

    protected void BindRequirements()
    {
        foreach (var option in _options)
        {
            List<Configurable<bool>[]> groups = [];

            foreach (var group in option.Requirements)
            {
                Configurable<bool>[] masters = [.. group.Where(_entries.ContainsKey)];

                if (masters.Length > 0)
                {
                    groups.Add(masters);
                }
            }

            if (groups.Count > 0)
            {
                BindMasters(option, [.. groups]);
            }
        }
    }

    protected void BeginTab(OpTab? tab)
    {
        EndTab();

        _currentTab = tab;
        _pos = new(_marginX.x, CanvasSize);
        _currentColumn = 0;

        AddLineBreak();
    }

    protected void EndTab()
    {
        if (_currentTab == null)
        {
            return;
        }

        ResetColumn();

        UIelement[] elements = [.. _elements];
        float height = CanvasSize - _pos.y + Padding;

        _elements.Clear();

        if (height > CanvasSize)
        {
            Vector2 offset = new(0f, height - CanvasSize);

            foreach (var element in elements)
            {
                element?.SetPos(element.GetPos() + offset);
            }

            OpScrollBox scrollBox = new(_currentTab, height, false, true);

            scrollBox.AddItems(elements);
        }
        else
        {
            _currentTab.AddItems(elements);
        }

        _currentTab = null;
    }

    protected void SetColumns(int columns)
    {
        ResetColumn();

        _columns = columns;
    }

    protected void ResetColumn()
    {
        if (_currentColumn > 0)
        {
            AddLineBreak(1.5f);
        }
    }

    protected void AddColumn(float span = 1f)
    {
        _pos.x += ElementWidth * span;

        if ((_currentColumn += span) > _columns - 0.5f)
        {
            ResetColumn();
        }
    }

    protected void AddLineBreak(float spacingModifier = 1f)
    {
        _pos.x = _marginX.x;
        _pos.y -= spacingModifier * Spacing;
        _currentColumn = 0;
    }

    protected OpLabel? AddLabel(string text, bool bigText = false, FLabelAlignment alignment = FLabelAlignment.Center, bool enabled = true)
    {
        if (_currentTab == null || !enabled)
        {
            return null;
        }

        ResetColumn();

        float height = (bigText ? 1.5f : 1f) * Spacing;

        if (bigText)
        {
            AddLineBreak(0.5f);
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

        AddElements(label);
        AddLineBreak();

        return label;
    }

    protected OpLabel? AddLabel(string text, string value)
    {
        if (AddLabel(text) is { } label)
        {
            label.text = label.text.Replace(Translation.Placeholder, value);

            return label;
        }

        return null;
    }

    protected void AddTitle(string? title = null, string? description = null, FLabelAlignment alignment = FLabelAlignment.Center, bool enabled = true)
    {
        if (!enabled)
        {
            return;
        }

        ResetColumn();
        AddLineBreak();

        if (title != null)
        {
            AddLabel(title, bigText: true, alignment: alignment);
        }
        if (description != null)
        {
            AddLabel(description, alignment: alignment);
        }
    }

    protected OpCheckBox? AddCheckBox(Configurable<bool> configurable, string? text = null, bool enabled = true)
    {
        if (!enabled)
        {
            return null;
        }

        OpCheckBox checkBox = new(
            configurable,
            new Vector2(_pos.x, _pos.y - Spacing * 0.5f)
        );

        return AddBox(configurable, checkBox, text);
    }

    protected OpFloatSlider? AddFloatSlider(Configurable<float> configurable, string? text = null, float span = 1f, float min = 0f, float max = 1f, byte decimals = 2, bool enabled = true)
    {
        if (!enabled || !BeginElement(span))
        {
            return null;
        }

        string description = Translate(configurable.Description ?? "");
        float labelWidth = 0f;
        OpLabel? label = null;

        if (text != null)
        {
            labelWidth = ElementWidth;

            label = new(
                new Vector2(_pos.x + Spacing + Gap, _pos.y - Spacing * 0.5f),
                new(ElementWidth - Gap, Spacing),
                Translate(text),
                FLabelAlignment.Left
            )
            {
                description = description
            };

            AddElements(label);
        }

        OpFloatSlider slider = new(
            configurable,
            new Vector2(_pos.x + Gap + labelWidth, _pos.y - Spacing * 0.5f - 3f),
            (int)(ElementWidth * span - Gap * 2f - labelWidth),
            decimals
        )
        {
            description = description,
            min = min,
            max = max,
        };

        AddColumn(span);

        AddElements(slider);
        Register(configurable, slider, label);

        return slider;
    }

    protected OpSimpleButton? AddSimpleButton(string text, string description, Action action, float span = 1f)
    {
        if (!BeginElement(span))
        {
            return null;
        }

        OpSimpleButton simpleButton = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new(ElementWidth * span - Gap * 2f, Spacing),
            Translate(text)
        )
        {
            description = Translate(description)
        };

        simpleButton.OnClick += delegate
        {
            action();
        };

        AddColumn(span);

        AddElements(simpleButton);

        return simpleButton;
    }

    private bool BeginElement(float span)
    {
        if (_currentTab == null)
        {
            return false;
        }

        if (_currentColumn > _columns - span + 0.5f)
        {
            ResetColumn();
        }

        return true;
    }

    private void AddElements(params UIelement[] elements)
    {
        if (_currentTab == null)
        {
            return;
        }

        _elements.AddRange(elements);
    }

    private T? AddBox<T>(ConfigurableBase configurableBase, T box, string? text) where T : UIfocusable
    {
        if (!BeginElement(1f))
        {
            return null;
        }

        string description = Translate(configurableBase.Description ?? "");

        box.description = description;

        OpLabel? label = null;

        if ((text ??= configurableBase.Label) != null)
        {
            label = new(
                new Vector2(_pos.x + Spacing + Gap, _pos.y - Spacing * 0.5f),
                new(ElementWidth - Spacing - Gap, Spacing),
                Translate(text),
                FLabelAlignment.Left
            )
            {
                description = description
            };

            AddElements(box, label);
        }
        else
        {
            AddElements(box);
        }

        AddColumn();
        Register(configurableBase, box, label);

        return box;
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
        return _entries.TryGetValue(master, out OptionEntry entry) && !entry.Focusable.greyedOut && (entry.Focusable is not OpCheckBox checkBox || checkBox.GetValueBool());
    }

    private string Describe(Configurable<bool>[] group)
    {
        string[] labels = [.. group.Select(master => Translate(master.Label ?? "")).Distinct()];
        string listed = string.Join(", ", labels);

        return labels.Length > 1 ? Translate("one of <PLACEHOLDER>").Replace(Translation.Placeholder, listed) : listed;
    }

    private void BindMasters(ConfigurableBase target, Configurable<bool>[][] groups)
    {
        if (groups.Length == 0 || !_entries.TryGetValue(target, out OptionEntry entry))
        {
            return;
        }

        void updateTarget()
        {
            string[] missing = [.. groups.Where(group => group.All(master => !IsMasterActive(master))).Select(Describe)];
            bool greyedOut = missing.Length > 0;

            entry.SetRequirement(greyedOut
                ? Translate("Turn on <PLACEHOLDER> first to use this option.").Replace(Translation.Placeholder, string.Join(", ", missing))
                : "");

            greyedOut |= entry.IsFailed;

            if (entry.Focusable.greyedOut != greyedOut)
            {
                entry.Focusable.greyedOut = greyedOut;

                Propagate(target);
            }
        }

        foreach (var master in groups.SelectMany(group => group))
        {
            if (_entries.TryGetValue(master, out OptionEntry masterEntry))
            {
                masterEntry.Focusable.OnChange += updateTarget;

                _propagates[master] = _propagates.TryGetValue(master, out Action propagate) ? propagate + updateTarget : updateTarget;
            }
        }

        updateTarget();
    }

    private sealed class OptionEntry(UIfocusable focusable, OpLabel? label, string description)
    {
        private string _mark = "";

        private string _requirement = "";

        public UIfocusable Focusable => focusable;

        public OpLabel? Label => label;

        public bool IsFailed { get; private set; }

        public void SetMark(string text, bool failed)
        {
            _mark = text;
            IsFailed |= failed;

            Refresh();
        }

        public void SetRequirement(string text)
        {
            _requirement = text;

            Refresh();
        }

        private void Refresh()
        {
            string[] notes = [description, _mark, _requirement];
            string text = string.Join("\n", notes.Where(note => note.Length > 0));

            Focusable.description = text;

            if (Label is { } opLabel)
            {
                opLabel.description = text;
            }
        }
    }
}

internal sealed class RemixMenu : RemixMenuBuilder
{
    public static readonly RemixMenu Instance = new();

    private OpLabel? _presetLabel;

    public RemixMenu()
    {
        Config.Bind(this);
    }

    public override void Initialize()
    {
        base.Initialize();

        OpTab presetTab = new(this, Translate("Presets"));
        OpTab friendTab = new(this, Translate("Friends"));
        OpTab gameplayTab = new(this, Translate("Gameplay"));
        OpTab progressionTab = new(this, Translate("Progression"));
        OpTab diagnosticsTab = new(this, Translate("Diagnostics"));

        Tabs = [presetTab, friendTab, gameplayTab, progressionTab, diagnosticsTab];

        BeginTab(presetTab);
        AddLabel("A preset sets every option at once.");
        AddLabel("Values are only laid out; apply the settings to keep them.");
        AddLabel("Ratios and durations are left as they were.");

        SetColumns(5);
        foreach (var preset in Preset.Presets)
        {
            AddSimpleButton(preset.Name, preset.Description, () =>
            {
                preset.Apply();
                RefreshPreset();
            });
        }
        _presetLabel = AddLabel("Current Preset");

        BeginTab(friendTab);
        AddLabel("Ripple Friends do not interfere with each other's trajectories.");
        AddLabel("This choice creates no immediate ripples, but determines who the forthcoming phenomena will reach.");
        AddLabel("Ripples flow bidirectionally to one another, excluding oneself.");

        AddTitle("Base");
        SetColumns(4);
        AddCheckBox(Config.FriendSlugcat);
        AddCheckBox(Config.FriendCreature);
        AddCheckBox(Config.FriendNeutralCreature);
        AddCheckBox(Config.FriendIterator);

        AddTitle("Extended");
        SetColumns(4);
        AddCheckBox(Config.FriendChaining);
        AddCheckBox(Config.FriendGrabbed);
        AddCheckBox(Config.FriendGrabbedForce);
        AddCheckBox(Config.FriendArena);

        BeginTab(gameplayTab);
        AddLabel("Every option here decides what passes between Ripple Friends and what turns aside from them.");

        AddTitle("General");
        SetColumns(5);
        AddCheckBox(Config.Violence);
        AddCheckBox(Config.Collision);
        AddCheckBox(Config.Explosion);
        AddCheckBox(Config.Fear);
        AddCheckBox(Config.Grabbing);
        SetColumns(4);
        AddCheckBox(Config.Deaf);
        AddFloatSlider(Config.DeafRatio, span: 3f);
        AddCheckBox(Config.Blind);
        AddFloatSlider(Config.BlindRatio, span: 3f);
        AddCheckBox(Config.Hypothermia, enabled: ModManager.HypothermiaModule);
        AddFloatSlider(Config.HypothermiaRatio, span: 3f, enabled: ModManager.HypothermiaModule);
        AddCheckBox(Config.Forgiveness);
        AddFloatSlider(Config.ForgivenessRatio, span: 3f);

        AddTitle("Items");
        SetColumns(4);
        AddCheckBox(Config.Rock);
        AddCheckBox(Config.Spear);
        AddCheckBox(Config.ExplosiveSpear);
        AddCheckBox(Config.ElectricSpear, enabled: ModManager.MSC);
        AddCheckBox(Config.HellSpear, enabled: ModManager.MSC);
        AddCheckBox(Config.PoisonSpear, enabled: ModManager.Watcher);
        AddCheckBox(Config.LilyPuck, enabled: ModManager.MSC);
        AddCheckBox(Config.ScavengerBomb);
        AddCheckBox(Config.SingularityBomb, enabled: ModManager.MSC);
        AddCheckBox(Config.FireEgg, enabled: ModManager.MSC);
        AddCheckBox(Config.SporePlant);
        AddCheckBox(Config.Boomerang, enabled: ModManager.Watcher);
        AddCheckBox(Config.Mushroom);
        AddCheckBox(Config.FlareBomb);
        AddCheckBox(Config.PuffBall);
        AddCheckBox(Config.WaterNut);
        AddCheckBox(Config.FirecrackerPlant);
        AddCheckBox(Config.GraffitiBomb, enabled: ModManager.Watcher);
        AddCheckBox(Config.DangleFruit, enabled: ModManager.MSC);
        AddCheckBox(Config.JellyFish);
        AddCheckBox(Config.Pomegranate, enabled: ModManager.Watcher);
        AddCheckBox(Config.Snail);
        AddCheckBox(Config.TubeWorm);
        AddCheckBox(Config.Frog, enabled: ModManager.Watcher);

        AddTitle("Players");
        SetColumns(4);
        AddCheckBox(Config.GrabbingPlayer);
        AddFloatSlider(Config.GrabbingPlayerTime, span: 3f, max: 5f);
        AddCheckBox(Config.Wiggle);
        AddCheckBox(Config.Carry);
        AddCheckBox(Config.Mauling);
        AddCheckBox(Config.GourmandSlam, enabled: ModManager.MSC);
        AddCheckBox(Config.ArtificerParry, enabled: ModManager.MSC);
        AddCheckBox(Config.SaintTongue, enabled: ModManager.MSC);
        AddCheckBox(Config.SaintAttunement, enabled: ModManager.MSC);
        AddCheckBox(Config.WatcherRipple, enabled: ModManager.Watcher);

        AddTitle("Lizards");
        SetColumns(3);
        AddCheckBox(Config.LizardBite);
        AddCheckBox(Config.LizardTongue);
        AddCheckBox(Config.LizardSpit);
        AddCheckBox(Config.LizardBeam, enabled: ModManager.Watcher);
        AddCheckBox(Config.LizardBlizzard, enabled: ModManager.Watcher);
        AddCheckBox(Config.LizardPoison, enabled: ModManager.Watcher);

        AddTitle("Scavengers");
        SetColumns(2);
        AddCheckBox(Config.ScavengerShelter);
        AddCheckBox(Config.ScavengerTemplar, enabled: ModManager.Watcher);

        AddTitle("Iterators");
        SetColumns(4);
        AddCheckBox(Config.Moon);
        AddCheckBox(Config.MoonNeuron);
        AddCheckBox(Config.Pebbles);
        AddCheckBox(Config.PebblesPearl, enabled: ModManager.MSC);

        BeginTab(progressionTab);
        AddLabel("It might be too long a journey to traverse this harsh world alone.");
        AddLabel("Let the world hold its doors, and resonate with the cycles you might have shared.");
        AddLabel("However, reckless resonances before the cycles properly align will have consequences.");

        AddTitle("Thresholds");
        SetColumns(4);
        AddCheckBox(Config.Gate);
        AddFloatSlider(Config.GateTime, span: 3f, max: 5f);
        AddCheckBox(Config.GateForce);
        AddFloatSlider(Config.GateForceTime, span: 3f, max: 5f);
        AddCheckBox(Config.Passage);
        AddCheckBox(Config.TempleGuard);

        AddTitle("Resonance");
        SetColumns(3);
        AddCheckBox(Config.ResonanceGate);
        AddCheckBox(Config.ResonanceRoom);
        AddCheckBox(Config.ResonanceGrab);
        SetColumns(2);
        AddCheckBox(Config.ResonanceWarp);
        AddCheckBox(Config.ResonanceMend);
        SetColumns(4);
        AddCheckBox(Config.ResonanceCost);
        AddFloatSlider(Config.ResonanceCostRatio, span: 3f);
        AddCheckBox(Config.ResonanceAftershock);
        AddFloatSlider(Config.ResonanceAftershockRatio, span: 3f);
        AddCheckBox(Config.ResonanceEffect);

        BeginTab(diagnosticsTab);
        AddLabel("If a feature breaks, it turns itself off and marks its option accordingly.");
        AddLabel("Every option here only shows what the mod sees, and none of them change what it does.");
        AddLabel("Turn on what a bug report needs and reproduce the problem.");
        AddLabel("Then send <PLACEHOLDER> saved in the StreamingAssets folder.", HookDiagnostics.ReportFileName);

        AddTitle("Visualizer");
        SetColumns(3);
        AddCheckBox(Config.FriendLink);
        AddCheckBox(Config.FriendName);
        AddCheckBox(Config.FriendIcon);
        AddCheckBox(Config.OwnerLink);
        AddCheckBox(Config.OwnerName);
        AddCheckBox(Config.OwnerIcon);

        AddTitle("Report");
        SetColumns(1);
        AddCheckBox(Config.Debug);
        AddSimpleButton("Open Report", "Shows the saved report in a file browser.", HookDiagnostics.OpenReport);

        EndTab();

        BindRequirements();

        Preset.Watch(RefreshPreset);
        RefreshPreset();
    }

    private void RefreshPreset()
    {
        _presetLabel?.text = Translate("Current Preset") + ": " + Translate(Preset.Current?.Name ?? "NONE");
    }
}
