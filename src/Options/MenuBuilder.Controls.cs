using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Options;

public abstract partial class MenuBuilder
{
    public OpCheckBox? AddCheckBox(Configurable<bool> configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        if (!enabled)
        {
            return null;
        }

        return AddBox(configurable, () => new OpCheckBox(
            configurable,
            new Vector2(_pos.x, _pos.y - Spacing * 0.5f)
        ), text, span);
    }

    public OpRadioButtonGroup? AddRadioButtonGroup(Configurable<int> configurable, bool enabled = true)
    {
        EndRadioButtonGroup();

        if (_currentTab == null || !enabled)
        {
            return null;
        }

        OpRadioButtonGroup radioButtonGroup = new(configurable)
        {
            description = Translate(configurable.Description ?? "")
        };

        Register(configurable, radioButtonGroup);

        return _radioButtonGroup = radioButtonGroup;
    }

    public OpRadioButton? AddRadioButton(string text, string? description = null, float span = 1f, bool enabled = true)
    {
        return AddRadioButton(text, out _, description, span, enabled);
    }

    public OpSlider? AddIntSlider(Configurable<int> configurable, string? text = null, float span = 1f, int min = 0, int max = 10, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpSlider(
            configurable,
            layout.Position - new Vector2(0f, 3f),
            (int)layout.Width
        )
        {
            description = layout.Description,
            min = min,
            max = max,
        });
    }

    public OpSliderTick? AddIntSliderTick(Configurable<int> configurable, string? text = null, float span = 1f, int min = 0, int max = 10, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpSliderTick(
            configurable,
            layout.Position - new Vector2(0f, 3f),
            (int)layout.Width,
            false
        )
        {
            description = layout.Description,
            min = min,
            max = max,
        });
    }

    public OpFloatSlider? AddFloatSlider(Configurable<float> configurable, string? text = null, float span = 1f, float min = 0f, float max = 1f, byte decimals = 2, int increment = 1, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpFloatSlider(
            configurable,
            layout.Position - new Vector2(0f, 3f),
            (int)layout.Width,
            decimals
        )
        {
            description = layout.Description,
            min = min,
            max = max,
            Increment = increment,
        });
    }

    public OpDragger? AddDragger(Configurable<int> configurable, string? text = null, float span = 1f, int min = 0, int max = 10, bool enabled = true)
    {
        if (!enabled)
        {
            return null;
        }

        return AddBox(configurable, () => new OpDragger(
            configurable,
            new Vector2(_pos.x, _pos.y - Spacing * 0.5f)
        )
        {
            min = min,
            max = max,
        }, text, span);
    }

    public OpUpdown? AddIntUpdown(Configurable<int> configurable, string? text = null, float span = 1f, int increment = 1, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpUpdown(
            configurable,
            layout.Position,
            layout.Width
        )
        {
            description = layout.Description,
            Increment = increment,
        });
    }

    public OpUpdown? AddFloatUpdown(Configurable<float> configurable, string? text = null, float span = 1f, byte decimals = 2, int increment = 1, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpUpdown(
            configurable,
            layout.Position,
            layout.Width,
            decimals
        )
        {
            description = layout.Description,
            Increment = increment,
        });
    }

    public OpTextBox? AddTextBox(ConfigurableBase configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpTextBox(
            configurable,
            layout.Position,
            layout.Width
        )
        {
            description = layout.Description
        });
    }

    public OpKeyBinder? AddKeyBinder(Configurable<KeyCode> configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        return AddControl(configurable, text, span, enabled, layout => new OpKeyBinder(
            configurable,
            layout.Position,
            new(layout.Width, Spacing)
        )
        {
            description = layout.Description
        });
    }

    public OpColorPicker? AddColorPicker(Configurable<Color> configurable, string? text = null, float? span = null, bool enabled = true)
    {
        if (_currentTab == null || !enabled)
        {
            return null;
        }

        BeginBlock(span);

        string description = Translate(configurable.Description ?? "");
        float width = BlockWidth(span);
        OpLabel? label = text == null ? null : AddBlockLabel(text, description, width);
        float top = _pos.y + (label == null ? 0f : Spacing * 0.5f);
        OpColorPicker colorPicker = new(configurable, new Vector2(_pos.x + Gap, top - 150f))
        {
            description = description
        };

        AddElements(colorPicker);
        Register(configurable, colorPicker, label);
        _pos.y = top - 150f;
        AddRow(0.5f);

        return colorPicker;
    }

    private protected OpRadioButton? AddRadioButton(string text, out OpLabel? label, string? description = null, float span = 1f, bool enabled = true)
    {
        label = null;

        if (_radioButtonGroup == null || !enabled || !BeginElement(span))
        {
            return null;
        }

        string note = Translate(description ?? "");
        OpRadioButton radioButton = new(_pos.x, _pos.y - Spacing * 0.5f)
        {
            description = note
        };

        label = new(
            new Vector2(_pos.x + Spacing + Gap, _pos.y - Spacing * 0.5f),
            new(ElementWidth * span - Spacing - Gap, Spacing),
            Translate(text),
            FLabelAlignment.Left
        )
        {
            description = note
        };

        AddColumn(span);
        AddElements(radioButton, label);
        _radioButtons.Add(radioButton);

        return radioButton;
    }

    private protected SectionFilter? AddSearchBox(string key, string text, string description, float span = 1f)
    {
        if (!BeginElement(span))
        {
            return null;
        }

        string note = Translate(description);

        AddControlLabel(note, text, span, out float labelWidth);

        SectionFilter filter = _filter ??= new();
        OpTextBox searchBox = new(
            Bind(key, "", new ConfigurableInfo(note)),
            new Vector2(_pos.x + Gap + labelWidth, _pos.y - Spacing * 0.5f),
            ElementWidth * span - Gap * 2f - labelWidth
        )
        {
            description = note,
            allowSpace = true,
        };
        string? pending = null;

        searchBox.OnChange += delegate
        {
            pending = searchBox.value;
        };

        _externals.Add(delegate
        {
            if (pending is { } query)
            {
                pending = null;
                filter.Apply(query);
            }
        });

        filter.Restore(searchBox.value);
        AddColumn(span);
        AddElements(searchBox);

        return filter;
    }

    private T? AddBox<T>(ConfigurableBase configurableBase, Func<T> create, string? text, float span) where T : UIfocusable
    {
        if (!BeginElement(span))
        {
            return null;
        }

        string description = Translate(configurableBase.Description ?? "");
        T box = create();

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

    private T? AddControl<T>(ConfigurableBase configurable, string? text, float span, bool enabled, Func<ControlLayout, T> create) where T : UIfocusable
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        return CompleteControl(configurable, create(layout), layout.Label, span);
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

    private readonly struct ControlLayout(string description, OpLabel? label, Vector2 position, float width)
    {
        internal string Description { get; } = description;

        internal OpLabel? Label { get; } = label;

        internal Vector2 Position { get; } = position;

        internal float Width { get; } = width;
    }
}
