using Menu.Remix.MixedUI;
using MoreSlugcats;
using AbsoluteFriends.Diagnostics;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using UnityEngine;

namespace AbsoluteFriendsExample;

internal sealed class RemixMenu : MenuBuilder
{
    // An independent Remix menu may inherit MenuBuilder for its layout helpers or OptionInterface to use the standard API directly.
    internal static readonly RemixMenu Instance = new();

    // ListItem keeps persisted values stable while display names can be translated.
    private static readonly ListItem[] styles = [
        new("Ripple", Translate("Ripple Purple")),
        new("Gold", Translate("Karamic Gold")),
    ];

    private static readonly ListItem[] namedCreatures = [
        new("lizard", Translate("Lizard")),
        new("scavenger", Translate("Scavenger")),
        new("iterator", Translate("Iterator")),
    ];

    private RemixMenu() => Config.Bind(this);

    public override void Initialize()
    {
        base.Initialize();

        OpTab overviewTab = new(this, Translate("Overview"));
        OpTab uiGalleryTab = new(this, Translate("UI Gallery"));

        Tabs = [overviewTab, uiGalleryTab];

        // A long tab becomes scrollable automatically. Wrap each tab's controls between BeginTab and EndTab.
        BeginTab(overviewTab);
        BuildOverviewTab();

        // EndTab();

        // BeginTab() automatically calls EndTab() so you could call EndTab() once after last tab is completed.
        BeginTab(uiGalleryTab);
        BuildUIGalleryTab();

        EndTab();

        // Apply Configurable.Require dependencies after every dependent control has been registered.
        BindRequirements();
    }

    private static void TestButton()
    {
        Reporter.GetLogger(Plugin.Name).LogInfo("The example sound button was pressed.");

        if (!Config.Announcements.IsActive || Custom.rainWorld?.processManager?.currentMainLoop is not Menu.Menu menu)
        {
            return;
        }

        menu.PlaySound(MoreSlugcatsEnums.MSCSoundID.Inv_GO, 0f, Config.Volume.Value, 1f);
    }

    private static void LogAction()
    {
        Reporter.GetLogger(Plugin.Name).LogInfo("An example MenuBuilder control was activated.");
    }

    private static void SetButtonColor(OpSimpleButton button, string style)
    {
        Color color = style == "Gold" ? Palette.Secondary : Palette.Primary;

        button.colorEdge = color;
        button.colorFill = color;
    }

    private void BuildOverviewTab()
    {
        AddTitle("Plugin Overview", "Adds a profile-aware addon and an independent Remix menu.");
        AddParagraph("This tab uses MenuBuilder for the plugin's own settings.", 55f);
        AddLabel("Addon options: Absolute Friends > Addons", alignment: FLabelAlignment.Right);

        SetColumns(4);
        AddNote("Plugin: <PLACEHOLDER>", Plugin.Name, alignment: FLabelAlignment.Left, span: 3f);
        AddNote("Version <PLACEHOLDER>", Plugin.Version);
        // AddRow can insert deliberate spacing after a complete row.
        AddRow(0.5f);

        SetColumns(4);
        AddCheckBox(Config.Announcements, "Sound Test", span: 2f);

        OpComboBox? style = AddComboBox(Config.Style, styles, "Button Color", span: 2f);

        AddIntSlider(Config.Volume, "Test Volume", span: 4f);

        OpSimpleButton? button = AddSimpleButton("Play Sound", "Logs an action and plays the game-over sound.", TestButton, span: 4f);

        if (style != null && button != null)
        {
            SetButtonColor(button, style.value);

            style.OnChange += delegate
            {
                SetButtonColor(button, style.value);
            };
        }
    }

    private void BuildUIGalleryTab()
    {
        AddTitle("UI Gallery", "Shows every public MenuBuilder control and overload.");
        AddParagraph("Remix saves these values; gameplay ignores them.", 30f);
        AddRow(0.5f);

        // AddContainer draws a box around the controls added by its callback and sizes the box to fit them.
        AddContainer(delegate
        {
            // Boolean controls.
            SetColumns(4);
            AddCheckBox(Config.CheckBoxValue, "Check Box", span: 2f);
            // AddColumn can reserve the remainder of a row without creating an element.
            AddColumn(2f);
            AddRadioButtonGroup(Config.RadioChoice);
            AddRadioButton("Ripple Purple", "Selects value one.", span: 2f);
            AddRadioButton("Karamic Gold", "Selects value two.");
            AddRadioButton("Neutral", "Selects value three.");
            AddRow();

            // Numeric controls.
            SetColumns(4);
            AddIntSlider(Config.SliderValue, "Integer Slider", span: 2f);
            AddIntSliderTick(Config.TickedValue, "Ticked Slider", span: 2f, min: 2, max: 14);
            AddFloatSlider(Config.FloatRange, "Float Slider", span: 2f, min: -1f, max: 2f, decimals: 2, increment: 3);
            AddDragger(Config.DraggedValue, "Integer Dragger", span: 2f, min: 2, max: 8);
            AddIntUpdown(Config.IntegerStep, "Integer Up-Down", span: 2f, increment: 4);
            AddFloatUpdown(Config.DecimalStep, "Decimal Up-Down", span: 2f, decimals: 2, increment: 5);
            AddRow();

            // Action controls.
            SetColumns(4);
            AddSimpleButton("Play Sound", "Logs an action and plays the game-over sound.", TestButton, span: 2f);
            // A narrow image button can be aligned left, center, or right within its span.
            AddSimpleImageButton("FriendA", "Logs an image-button action.", LogAction, width: 24f, alignment: FLabelAlignment.Center);
            AddHoldButton("Hold to Log", "Hold to log an action.", LogAction);
            AddRow();

            // Text and key controls.
            SetColumns(4);
            AddTextBox(Config.TextValue, "Text Box", span: 2f);
            AddKeyBinder(Config.Shortcut, "Shortcut", span: 2f);
            SetColumns(4);
            // Compact selection controls. String arrays use identical stored and displayed values.
            AddComboBox(Config.SimpleChoice, ["Alpha", "Beta", "Gamma"], "Simple Choice", span: 2f);
            // ListItem keeps persisted values stable while display names can be translated.
            AddComboBox(Config.NamedChoice, styles, "Named Choice", span: 2f);
            // Enum configurables infer their values; SpecialEnum loads values from Rain World's resources.
            AddResourceSelector(Config.ResourceChoice, "Enum Selector", span: 2f);
            AddResourceSelector(Config.RegionChoice, OpResourceSelector.SpecialEnum.Regions, "Region Selector", span: 2f);
            AddRow();

            // Expanded selection controls.
            SetColumns(4);
            AddListBox(
                Config.ListedChoice,
                ["Lizard", "Scavenger", "Iterator", "Cicada", "Lantern Mouse", "Rain Deer", "Vulture", "Miros Bird", "Noodlefly", "Jetfish", "Yeek", "Slugcat"],
                "Choice List",
                visibleItems: 4,
                span: 3f
            );
            AddListBox(Config.NamedListedChoice, namedCreatures, "Named List", visibleItems: 3, span: 2f);
            AddColumn(3f);
            AddResourceList(Config.ResourceListChoice, "Enum List", visibleItems: 2, span: 1f);
            AddResourceList(Config.RegionListChoice, OpResourceSelector.SpecialEnum.Regions, "Region List", visibleItems: 3, span: 4f);
            AddRow();

            // Color control.
            SetColumns(4);
            AddColumn(1f);
            AddColorPicker(Config.Tint, "Tint", span: 2f);
        });
    }
}
