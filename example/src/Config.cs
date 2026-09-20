using RippleFriends.Options;
using UnityEngine;

namespace RippleFriendsExample;

internal enum ExampleResource
{
    Ripple,
    Gold,
}

// These ordinary Remix settings are independent from the profile-aware addon settings in AddonConfig.
internal static class Config
{
    internal static Configurable<bool> Announcements = null!;

    internal static Configurable<string> Style = null!;

    internal static Configurable<int> Volume = null!;

    internal static Configurable<bool> CheckBoxValue = null!;

    internal static Configurable<int> RadioChoice = null!;

    internal static Configurable<int> SliderValue = null!;

    internal static Configurable<int> TickedValue = null!;

    internal static Configurable<float> FloatRange = null!;

    internal static Configurable<int> DraggedValue = null!;

    internal static Configurable<int> IntegerStep = null!;

    internal static Configurable<float> DecimalStep = null!;

    internal static Configurable<string> TextValue = null!;

    internal static Configurable<KeyCode> Shortcut = null!;

    internal static Configurable<string> SimpleChoice = null!;

    internal static Configurable<string> NamedChoice = null!;

    internal static Configurable<ExampleResource> ResourceChoice = null!;

    internal static Configurable<string> RegionChoice = null!;

    internal static Configurable<string> ListedChoice = null!;

    internal static Configurable<string> NamedListedChoice = null!;

    internal static Configurable<ExampleResource> ResourceListChoice = null!;

    internal static Configurable<string> RegionListChoice = null!;

    internal static Configurable<Color> Tint = null!;

    // Collecting bindings here is optional; a menu may bind them directly in its constructor.
    internal static void Bind(OptionInterface oi)
    {
        Announcements = oi.config.Bind("Announcements", true, new ConfigurableInfo("Enables the test button sound.", tags: ["Sound Test"]));
        Style = oi.config.Bind("Style", "Ripple", new ConfigurableInfo("Selects the test button color.", tags: ["Button Color"]));
        Volume = oi.config.Bind("Volume", 5, new ConfigurableInfo("Sets the test sound volume.", tags: ["Test Volume"]))
            .Require(Announcements);

        CheckBoxValue = oi.config.Bind("CheckBoxValue", true, new ConfigurableInfo("A Boolean toggle.", tags: ["Check Box"]));
        RadioChoice = oi.config.Bind("BoundaryRadioChoice", 2, new ConfigurableInfo("A three-value radio group.", new ConfigAcceptableRange<int>(0, 2), tags: ["Radio Group"]));

        SliderValue = oi.config.Bind("SliderValue", 5, new ConfigurableInfo("An integer slider.", new ConfigAcceptableRange<int>(0, 10), tags: ["Integer Slider"]));
        TickedValue = oi.config.Bind("PositiveBoundaryTickedValue", 14, new ConfigurableInfo("An integer slider with ticks.", new ConfigAcceptableRange<int>(2, 14), tags: ["Ticked Slider"]));
        FloatRange = oi.config.Bind("BoundaryFloatRange", -1f, new ConfigurableInfo("A float slider at its minimum.", new ConfigAcceptableRange<float>(-1f, 2f), tags: ["Float Slider"]));
        DraggedValue = oi.config.Bind("BoundaryDraggedValue", 2, new ConfigurableInfo("An integer dragger.", new ConfigAcceptableRange<int>(2, 8), tags: ["Integer Dragger"]));
        IntegerStep = oi.config.Bind("PositiveBoundaryIntegerStep", 12, new ConfigurableInfo("An integer field with a step of four.", new ConfigAcceptableRange<int>(0, 12), tags: ["Integer Up-Down"]));
        DecimalStep = oi.config.Bind("BoundaryDecimalStep", 0f, new ConfigurableInfo("A decimal field.", new ConfigAcceptableRange<float>(0f, 2.5f), tags: ["Decimal Up-Down"]));

        TextValue = oi.config.Bind("BoundaryTextValue", "Edge", new ConfigurableInfo("A text field.", tags: ["Text Box"]));
        Shortcut = oi.config.Bind("Shortcut", KeyCode.None, new ConfigurableInfo("Binds a keyboard shortcut.", tags: ["Shortcut"]));

        SimpleChoice = oi.config.Bind("SimpleChoice", "Alpha", new ConfigurableInfo("Uses matching stored and display text.", tags: ["Simple Choice"]));
        NamedChoice = oi.config.Bind("NamedChoice", "Ripple", new ConfigurableInfo("Uses stable values and translated names.", tags: ["Named Choice"]));
        ResourceChoice = oi.config.Bind("BoundaryResourceChoice", ExampleResource.Gold, new ConfigurableInfo("Uses a custom enum.", tags: ["Enum Selector"]));
        RegionChoice = oi.config.Bind("RegionChoice", "SU", new ConfigurableInfo("Uses Rain World regions.", tags: ["Region Selector"]));

        ListedChoice = oi.config.Bind("BoundaryListedChoice", "Scavenger", new ConfigurableInfo("Uses matching stored and display values.", tags: ["Choice List"]));
        NamedListedChoice = oi.config.Bind("NamedListedChoice", "scavenger", new ConfigurableInfo("Uses stable list values and translated names.", tags: ["Named List"]));
        ResourceListChoice = oi.config.Bind("BoundaryResourceListChoice", ExampleResource.Gold, new ConfigurableInfo("Expands a custom enum.", tags: ["Enum List"]));
        RegionListChoice = oi.config.Bind("RegionListChoice", "HI", new ConfigurableInfo("Lists Rain World regions.", tags: ["Region List"]));

        Tint = oi.config.Bind("BoundaryTint", new Color(0f, 1f, 0.5f, 1f), new ConfigurableInfo("A color picker.", tags: ["Tint"]));
    }
}
