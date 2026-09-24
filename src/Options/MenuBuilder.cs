using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Options;

public abstract partial class MenuBuilder : OptionInterface
{
    private const float CanvasSize = 600f;

    private const float Padding = 30f;

    private const float Spacing = 24f;

    private const float Gap = 6f;

    private static readonly Vector2 _defaultMarginX = new(Padding, CanvasSize - Padding);

    private readonly List<UIelement> _elements = [];

    private readonly List<ConfigurableBase> _options = [];

    private readonly Dictionary<ConfigurableBase, OptionEntry> _entries = [];

    private readonly Dictionary<ConfigurableBase, Action> _propagates = [];

    private readonly List<Action> _externals = [];

    private readonly List<OpRadioButton> _radioButtons = [];

    private readonly List<Section> _sections = [];

    private OpTab? _currentTab;

    private OpRadioButtonGroup? _radioButtonGroup;

    private Section? _section;

    private SectionFilter? _filter;

    private OpRect? _box;

    private int _boxIndex;

    private int _headerIndex = -1;

    private float _headerBottom;

    private Vector2 _marginX = _defaultMarginX;

    private Vector2 _pos = new();

    private float _edge = CanvasSize;

    private int _columns = 4;

    private float _currentColumn = 0f;

    private float Width => _marginX.y - _marginX.x;

    private float ElementWidth => Width / _columns;

    public override void Initialize()
    {
        base.Initialize();

        _currentTab = null;
        _radioButtonGroup = null;
        _section = null;
        _filter = null;
        _box = null;
        _marginX = _defaultMarginX;
        _headerIndex = -1;

        _elements.Clear();
        _options.Clear();
        _entries.Clear();
        _propagates.Clear();
        _externals.Clear();
        _radioButtons.Clear();
        _sections.Clear();
    }

    public override void Update()
    {
        base.Update();

        foreach (var external in _externals)
        {
            external();
        }

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

    internal Configurable<T> Bind<T>(string key, T defaultValue, ConfigurableInfo? info = null)
    {
        return config.configurables.TryGetValue(key, out ConfigurableBase configurable) && configurable is Configurable<T> bound
            ? bound
            : config.Bind(key, defaultValue, info);
    }

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

    protected void BeginTab(OpTab? tab)
    {
        EndTab();

        _currentTab = tab;
        _pos = new(_marginX.x, CanvasSize);
        _edge = CanvasSize;
        _currentColumn = 0;

        AddRow();
    }

    protected void EndTab()
    {
        if (_currentTab == null)
        {
            return;
        }

        EndRadioButtonGroup();
        EndSection();
        FinishRow();

        UIelement[] elements = [.. _elements];
        UIelement[] header = _headerIndex < 0 ? [] : [.. elements.Take(_headerIndex)];
        UIelement[] body = _headerIndex < 0 ? elements : [.. elements.Skip(_headerIndex)];
        float view = _headerIndex < 0 ? CanvasSize : _headerBottom;
        float height = view - _pos.y + Spacing * 0.5f;
        OpScrollBox? scrollBox = null;

        _elements.Clear();

        if (height > view)
        {
            Vector2 offset = new(0f, height - view);

            foreach (var element in body)
            {
                element?.SetPos(element.GetPos() + offset);
            }

            scrollBox = new(new Vector2(0f, 0f), new Vector2(CanvasSize, view), height, false, false, true);
            _currentTab.AddItems(scrollBox);
            scrollBox.AddItems(body);
        }
        else
        {
            _currentTab.AddItems(body);
        }

        if (header.Length > 0)
        {
            _currentTab.AddItems(header);
        }

        foreach (var element in elements)
        {
            Snap(element);
        }

        if (_sections.Count > 0)
        {
            if (_filter is { } filter)
            {
                foreach (var section in _sections)
                {
                    section.Anchor();
                }

                filter.Bind(this, scrollBox, height, view);
                _externals.Add(filter.UpdateLayout);
            }

            _sections.Clear();
        }

        _filter = null;
        _headerIndex = -1;
        _currentTab = null;
    }

    private protected void PinHeader()
    {
        if (_currentTab == null)
        {
            return;
        }

        FinishRow();
        _headerIndex = _elements.Count;
        _headerBottom = _pos.y;
        _edge = _pos.y;
        _pos.y -= Spacing * 0.5f;
    }

    private protected void BeginFold(Func<bool> unfolded)
    {
        if (_section is not { } section || _box is not { } box)
        {
            return;
        }

        FinishRow();
        _pos.y = _edge - Spacing;
        section.Fold(box, unfolded, _elements.Count - section.Index, _edge - Gap);
    }

    private protected void BeginSection(string? name = null, string? alias = null, Func<bool>? gate = null)
    {
        BeginSection([name, alias], gate);
    }

    private protected void BeginSection(string?[] names, Func<bool>? gate = null)
    {
        EndSection();

        if (_currentTab == null)
        {
            return;
        }

        _section = new(names, _elements.Count, _pos.y, gate);
    }

    private protected void BeginBox()
    {
        if (_currentTab == null || _box != null)
        {
            return;
        }

        FinishRow();
        _box = new(new Vector2(_marginX.x, _pos.y), new Vector2(Width, Spacing));
        AddElements(_box);
        _boxIndex = _elements.Count;
        _marginX = new(_marginX.x + Gap * 2f, _marginX.y - Gap * 2f);
        AddRow(0.5f);
    }

    private protected OpRect? EndBox()
    {
        if (_box is not { } box)
        {
            return null;
        }

        FinishRow();

        float top = box.GetPos().y;
        float bottom = float.MaxValue;

        foreach (var element in _elements.Skip(_boxIndex).Where(element => element.isRectangular))
        {
            top = Mathf.Max(top, element.GetPos().y + element.size.y);
            bottom = Mathf.Min(bottom, element.GetPos().y);
        }

        if (bottom > top)
        {
            bottom = _pos.y;
        }

        top += Gap;
        bottom -= Gap;
        _marginX = _defaultMarginX;
        _box = null;
        box.SetPos(new(box.GetPos().x, bottom));
        box.size = new(box.size.x, top - bottom);
        _edge = bottom;
        _pos.y = bottom;
        AddRow(0.5f);

        return box;
    }

    private protected void Watch(Action update)
    {
        _externals.Add(update);
    }

    private static float ListBoxHeight(int visibleItems)
    {
        return visibleItems * 20f + 34f;
    }

    private static void Snap(UIelement element)
    {
        element.lastScreenPos = element.ScreenPos;
    }

    private OpLabel? AddLabel(string text, string? value, bool bigText, FLabelAlignment alignment, bool enabled)
    {
        if (_currentTab == null || !enabled)
        {
            return null;
        }

        FinishRow();

        string content = value == null ? Translate(text) : Translate(text).FillPlaceholders(value);
        OpLabel label = new(
            new(_pos.x, _pos.y - Spacing * 0.5f),
            new(Width, Spacing),
            content,
            alignment,
            bigText
        )
        {
            autoWrap = true,
        };

        label.Change();
        AddElements(label);
        AddRow();

        return label;
    }

    private OpLabel? AddNote(string text, string? value, FLabelAlignment alignment, float span, bool enabled)
    {
        if (!enabled || !BeginElement(span))
        {
            return null;
        }

        string content = Translate(text);

        if (value != null)
        {
            content = content.FillPlaceholders(value);
        }

        OpLabel label = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new(ElementWidth * span - Gap * 2f, Spacing),
            content,
            alignment
        );

        AddColumn(span);
        AddElements(label);

        return label;
    }

    private T? AddBox<T>(ConfigurableBase configurableBase, T box, string? text, float span) where T : UIfocusable
    {
        if (!BeginElement(span))
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
                new(ElementWidth * span - Spacing - Gap, Spacing),
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

        AddColumn(span);
        Register(configurableBase, box, label);

        return box;
    }

    private T? AddListControl<T>(
        ConfigurableBase configurable,
        string? text,
        ushort visibleItems,
        float? span,
        bool enabled,
        Func<Vector2, float, T> create,
        Func<T, int> measure
    ) where T : UIfocusable
    {
        if (_currentTab == null || !enabled || visibleItems == 0)
        {
            return null;
        }

        BeginBlock(span);

        string description = Translate(configurable.Description ?? "");
        float width = BlockWidth(span);

        if (text != null)
        {
            AddBlockLabel(text, description, width);
        }

        float top = _pos.y + Spacing * 0.5f;
        T list = create(new Vector2(_pos.x + Gap, _pos.y), width - Gap * 2f);

        list.description = description;

        float height = ListBoxHeight(measure(list));

        list.SetPos(new Vector2(list.GetPos().x, top - height));

        AddElements(list);
        Register(configurable, list);

        _pos.y = top - height;

        AddRow(0.5f);

        return list;
    }

    private OpLabel AddBlockLabel(string text, string description, float width)
    {
        OpLabel label = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new Vector2(width - Gap * 2f, Spacing),
            Translate(text),
            FLabelAlignment.Left
        )
        {
            description = description
        };

        AddElements(label);
        _pos.y -= Spacing;

        return label;
    }

    private void BeginBlock(float? span)
    {
        if (span is { } blockSpan)
        {
            BeginElement(blockSpan);
        }
        else
        {
            FinishRow();
        }
    }

    private float BlockWidth(float? span)
    {
        return span is > 0f ? Mathf.Min(Width, ElementWidth * span.Value) : Width;
    }

    private void EndSection()
    {
        if (_section is not { } section)
        {
            return;
        }

        EndBox();
        section.Close([.. _elements.Skip(section.Index)], _pos.y);
        _sections.Add(section);
        _section = null;
    }

    private void EndRadioButtonGroup()
    {
        if (_radioButtonGroup == null)
        {
            return;
        }

        _radioButtonGroup.SetButtons([.. _radioButtons]);
        _currentTab?.AddItems(_radioButtonGroup);
        _radioButtonGroup = null;
        _radioButtons.Clear();
    }

    private bool TryBeginControl(ConfigurableBase configurable, string? text, float span, bool enabled, out ControlLayout layout)
    {
        layout = default;

        if (!enabled || !BeginElement(span))
        {
            return false;
        }

        string description = Translate(configurable.Description ?? "");
        OpLabel? label = AddControlLabel(description, text, span, out float labelWidth);

        layout = new(
            description,
            label,
            new Vector2(_pos.x + Gap + labelWidth, _pos.y - Spacing * 0.5f),
            ElementWidth * span - Gap * 2f - labelWidth
        );

        return true;
    }

    private T CompleteControl<T>(ConfigurableBase configurable, T control, OpLabel? label, float span) where T : UIfocusable
    {
        AddColumn(span);
        AddElements(control);
        Register(configurable, control, label);

        return control;
    }

    private OpLabel? AddControlLabel(string description, string? text, float span, out float labelWidth)
    {
        labelWidth = 0f;

        if (text == null)
        {
            return null;
        }

        labelWidth = Mathf.Min(ElementWidth, ElementWidth * span * 0.5f);

        OpLabel label = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new(labelWidth - Gap * 2f, Spacing),
            Translate(text),
            FLabelAlignment.Left
        )
        {
            description = description
        };

        AddElements(label);

        return label;
    }

    private bool BeginElement(float span)
    {
        if (_currentTab == null)
        {
            return false;
        }

        ValidateSpan(span);

        if (_currentColumn > _columns - span + 0.5f)
        {
            FinishRow();
        }

        return true;
    }

    private void ValidateSpan(float span)
    {
        if (span <= 0f || span > _columns || float.IsNaN(span) || float.IsInfinity(span))
        {
            throw new ArgumentOutOfRangeException(nameof(span), $"Span must be finite, greater than zero, and no larger than the current column count ({_columns}).");
        }
    }

    private void AddElements(params UIelement[] elements)
    {
        if (_currentTab == null)
        {
            return;
        }

        _elements.AddRange(elements);

        foreach (var element in elements)
        {
            _edge = Mathf.Min(_edge, element.GetPos().y);
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

    private readonly struct ControlLayout(string description, OpLabel? label, Vector2 position, float width)
    {
        internal string Description { get; } = description;

        internal OpLabel? Label { get; } = label;

        internal Vector2 Position { get; } = position;

        internal float Width { get; } = width;
    }
}
