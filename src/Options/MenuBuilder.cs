using AbsoluteFriends.Hooks;
using AbsoluteFriends.Utils;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
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

    private Configurable<bool>? _buttonRequirement;

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
        _buttonRequirement = null;
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
            scrollBox.OnUpdate += () => FocusScrollChild(scrollBox, body);
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

    private protected void Watch(Action update)
    {
        _externals.Add(update);
    }

    internal void WithButtonRequirement(Configurable<bool>? requirement, Action build)
    {
        Configurable<bool>? previous = _buttonRequirement;

        _buttonRequirement = requirement;

        try
        {
            build();
        }
        finally
        {
            _buttonRequirement = previous;
        }
    }

    private static void Snap(UIelement element)
    {
        element.lastScreenPos = element.ScreenPos;
    }

    private static void FocusScrollChild(OpScrollBox scrollBox, UIelement[] body)
    {
        if (!ReferenceEquals(ConfigContainer.FocusedElement, scrollBox)
            || scrollBox.held
            || Custom.rainWorld?.processManager?.menuesMouseMode == true)
        {
            return;
        }

        UIfocusable? firstVisible = body.OfType<UIfocusable>()
            .FirstOrDefault(item => !item.IsInactive && item.CurrentlyFocusableNonMouse && OpScrollBox.IsChildVisible(item))
            ?? body.OfType<UIfocusable>().FirstOrDefault(item => !item.IsInactive && item.CurrentlyFocusableNonMouse);

        if (firstVisible != null)
        {
            ConfigConnector.FocusNewElement(firstVisible);
        }
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

}
