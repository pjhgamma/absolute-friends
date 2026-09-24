using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Options;

public abstract partial class MenuBuilder
{
    public void SetColumns(int columns)
    {
        if (columns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), "Column count must be greater than zero.");
        }

        FinishRow();
        _columns = columns;
    }

    public void AddColumn(float span = 1f)
    {
        ValidateSpan(span);
        _pos.x += ElementWidth * span;

        if ((_currentColumn += span) > _columns - 0.5f)
        {
            FinishRow();
        }
    }

    public void AddRow(float modifier = 1f)
    {
        _pos.x = _marginX.x;
        _pos.y -= modifier * Spacing;
        _currentColumn = 0;
    }

    public OpRect? AddContainer(Action content)
    {
        if (_currentTab == null || _box != null)
        {
            return null;
        }

        BeginBox();

        OpRect? container = null;

        try
        {
            content();
        }
        finally
        {
            container = EndBox();
        }

        return container;
    }

    public OpLabel? AddLabel(string text, bool bigText = false, FLabelAlignment alignment = FLabelAlignment.Center, bool enabled = true)
    {
        return AddLabel(text, null, bigText, alignment, enabled);
    }

    public OpLabel? AddLabel(string text, string value, FLabelAlignment alignment = FLabelAlignment.Center)
    {
        return AddLabel(text, value, false, alignment, true);
    }

    public OpLabel? AddNote(string text, FLabelAlignment alignment = FLabelAlignment.Right, float span = 1f, bool enabled = true)
    {
        return AddNote(text, null, alignment, span, enabled);
    }

    public OpLabel? AddNote(string text, string value, FLabelAlignment alignment = FLabelAlignment.Right, float span = 1f)
    {
        return AddNote(text, value, alignment, span, true);
    }

    public void AddTitle(string? title = null, string? description = null, FLabelAlignment alignment = FLabelAlignment.Center, bool enabled = true)
    {
        if (!enabled)
        {
            return;
        }

        FinishRow();
        _pos = new(_marginX.x, _edge - Spacing);

        if (title != null)
        {
            AddLabel(title, bigText: true, alignment: alignment);
        }
        if (description != null)
        {
            AddLabel(description, alignment: alignment);
        }
    }

    public OpLabelLong? AddParagraph(string text, float height, float? span = null, FLabelAlignment alignment = FLabelAlignment.Left, bool enabled = true)
    {
        if (_currentTab == null || !enabled || height <= 0f)
        {
            return null;
        }

        BeginBlock(span);

        float width = BlockWidth(span);
        OpLabelLong paragraph = new(
            new Vector2(_pos.x + Gap, _pos.y - height),
            new Vector2(width - Gap * 2f, height),
            Translate(text),
            true,
            alignment
        );

        AddElements(paragraph);
        _pos.y -= height;
        AddRow(0.5f);

        return paragraph;
    }

    public OpCheckBox? AddCheckBox(Configurable<bool> configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        if (!enabled)
        {
            return null;
        }

        OpCheckBox checkBox = new(
            configurable,
            new Vector2(_pos.x, _pos.y - Spacing * 0.5f)
        );

        return AddBox(configurable, checkBox, text, span);
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
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpSlider slider = new(
            configurable,
            layout.Position - new Vector2(0f, 3f),
            (int)layout.Width
        )
        {
            description = layout.Description,
            min = min,
            max = max,
        };

        return CompleteControl(configurable, slider, layout.Label, span);
    }

    public OpSliderTick? AddIntSliderTick(Configurable<int> configurable, string? text = null, float span = 1f, int min = 0, int max = 10, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpSliderTick slider = new(
            configurable,
            layout.Position - new Vector2(0f, 3f),
            (int)layout.Width,
            false
        )
        {
            description = layout.Description,
            min = min,
            max = max,
        };

        return CompleteControl(configurable, slider, layout.Label, span);
    }

    public OpFloatSlider? AddFloatSlider(Configurable<float> configurable, string? text = null, float span = 1f, float min = 0f, float max = 1f, byte decimals = 2, int increment = 1, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpFloatSlider slider = new(
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
        };

        return CompleteControl(configurable, slider, layout.Label, span);
    }

    public OpDragger? AddDragger(Configurable<int> configurable, string? text = null, float span = 1f, int min = 0, int max = 10, bool enabled = true)
    {
        if (!enabled)
        {
            return null;
        }

        OpDragger dragger = new(
            configurable,
            new Vector2(_pos.x, _pos.y - Spacing * 0.5f)
        )
        {
            min = min,
            max = max,
        };

        return AddBox(configurable, dragger, text, span);
    }

    public OpUpdown? AddIntUpdown(Configurable<int> configurable, string? text = null, float span = 1f, int increment = 1, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpUpdown updown = new(
            configurable,
            layout.Position,
            layout.Width
        )
        {
            description = layout.Description,
            Increment = increment,
        };

        return CompleteControl(configurable, updown, layout.Label, span);
    }

    public OpUpdown? AddFloatUpdown(Configurable<float> configurable, string? text = null, float span = 1f, byte decimals = 2, int increment = 1, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpUpdown updown = new(
            configurable,
            layout.Position,
            layout.Width,
            decimals
        )
        {
            description = layout.Description,
            Increment = increment,
        };

        return CompleteControl(configurable, updown, layout.Label, span);
    }

    public OpSimpleButton? AddSimpleButton(string text, string description, Action action, float span = 1f)
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

    public OpSimpleImageButton? AddSimpleImageButton(string image, string description, Action action, float span = 1f, float? width = null, FLabelAlignment alignment = FLabelAlignment.Right)
    {
        if (!BeginElement(span))
        {
            return null;
        }

        float spanWidth = ElementWidth * span;
        float buttonWidth = width ?? spanWidth - Gap * 2f;
        float buttonX = _pos.x + alignment switch
        {
            FLabelAlignment.Left => Gap,
            FLabelAlignment.Center => (spanWidth - buttonWidth) * 0.5f,
            _ => spanWidth - buttonWidth - Gap,
        };
        OpSimpleImageButton imageButton = new(
            new Vector2(buttonX, _pos.y - Spacing * 0.5f),
            new Vector2(buttonWidth, Spacing),
            image
        )
        {
            description = Translate(description)
        };

        imageButton.OnClick += delegate
        {
            action();
        };

        AddColumn(span);
        AddElements(imageButton);

        return imageButton;
    }

    public OpHoldButton? AddHoldButton(string text, string description, Action action, float span = 1f)
    {
        if (!BeginElement(span))
        {
            return null;
        }

        OpHoldButton holdButton = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new Vector2(ElementWidth * span - Gap * 2f, Spacing),
            Translate(text)
        )
        {
            description = Translate(description)
        };

        holdButton.OnPressDone += delegate
        {
            action();
        };

        AddColumn(span);
        AddElements(holdButton);

        return holdButton;
    }

    public OpTextBox? AddTextBox(ConfigurableBase configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpTextBox textBox = new(
            configurable,
            layout.Position,
            layout.Width
        )
        {
            description = layout.Description
        };

        return CompleteControl(configurable, textBox, layout.Label, span);
    }

    public OpKeyBinder? AddKeyBinder(Configurable<KeyCode> configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpKeyBinder keyBinder = new(
            configurable,
            layout.Position,
            new(layout.Width, Spacing)
        )
        {
            description = layout.Description
        };

        return CompleteControl(configurable, keyBinder, layout.Label, span);
    }

    public OpComboBox? AddComboBox(Configurable<string> configurable, string[] choices, string? text = null, float span = 1f, bool enabled = true)
    {
        return AddComboBox(configurable, [.. choices.Select(choice => new ListItem(choice))], text, span, enabled);
    }

    public OpComboBox? AddComboBox(Configurable<string> configurable, ListItem[] choices, string? text = null, float span = 1f, bool enabled = true)
    {
        if (choices.Length == 0 || !TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpComboBox comboBox = new(
            configurable,
            layout.Position,
            layout.Width,
            [.. choices]
        )
        {
            description = layout.Description
        };

        return CompleteControl(configurable, comboBox, layout.Label, span);
    }

    public OpResourceSelector? AddResourceSelector(ConfigurableBase configurable, string? text = null, float span = 1f, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpResourceSelector resourceSelector = new(
            configurable,
            layout.Position,
            layout.Width
        )
        {
            description = layout.Description
        };

        return CompleteControl(configurable, resourceSelector, layout.Label, span);
    }

    public OpResourceSelector? AddResourceSelector(Configurable<string> configurable, OpResourceSelector.SpecialEnum resource, string? text = null, float span = 1f, bool enabled = true)
    {
        if (!TryBeginControl(configurable, text, span, enabled, out ControlLayout layout))
        {
            return null;
        }

        OpResourceSelector resourceSelector = new(
            configurable,
            layout.Position,
            layout.Width,
            resource
        )
        {
            description = layout.Description
        };

        return CompleteControl(configurable, resourceSelector, layout.Label, span);
    }

    public OpListBox? AddListBox(Configurable<string> configurable, string[] choices, string? text = null, ushort visibleItems = 5, float? span = null, bool enabled = true)
    {
        return AddListBox(configurable, [.. choices.Select(choice => new ListItem(choice))], text, visibleItems, span, enabled);
    }

    public OpListBox? AddListBox(Configurable<string> configurable, ListItem[] choices, string? text = null, ushort visibleItems = 5, float? span = null, bool enabled = true)
    {
        if (choices.Length == 0)
        {
            return null;
        }

        return AddListControl(
            configurable,
            text,
            visibleItems,
            span,
            enabled,
            (position, width) => new OpListBox(configurable, position, width, [.. choices], visibleItems, true),
            listBox => listBox._listHeight
        );
    }

    public OpResourceList? AddResourceList(ConfigurableBase configurable, string? text = null, ushort visibleItems = 5, float? span = null, bool enabled = true)
    {
        return AddListControl(
            configurable,
            text,
            visibleItems,
            span,
            enabled,
            (position, width) => new OpResourceList(configurable, position, width, visibleItems, true),
            resourceList => resourceList._listHeight
        );
    }

    public OpResourceList? AddResourceList(Configurable<string> configurable, OpResourceSelector.SpecialEnum resource, string? text = null, ushort visibleItems = 5, float? span = null, bool enabled = true)
    {
        return AddListControl(
            configurable,
            text,
            visibleItems,
            span,
            enabled,
            (position, width) => new OpResourceList(configurable, position, width, resource, visibleItems, true),
            resourceList => resourceList._listHeight
        );
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

    private void FinishRow()
    {
        if (_currentColumn > 0)
        {
            AddRow(1.5f);
        }
    }
}
