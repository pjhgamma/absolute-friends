using AbsoluteFriends.Options;

namespace AbsoluteFriends.Visualizer;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Relationship Overlay");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.Icons);
        menu.AddRadioButtonGroup(Config.IconStyle);
        menu.AddRadioButton("Creature Icon", "Uses creature artwork for relationship icons. Items keep their item icons.");
        menu.AddRadioButton("Symbol Icon", "Uses friendship symbols for creatures. Items keep their item icons.");
        menu.AddCheckBox(Config.Names);
        menu.AddCheckBox(Config.Links);

        menu.AddTitle("Shown Relationships");
        menu.SetColumns(4);
        menu.AddTickLabels(
            menu.AddIntSliderTick(Config.ShownFriendship, "Friendship", span: 4f, min: 0, max: 3),
            "Tracking",
            "Friendly",
            "Neutral",
            "Indirect"
        );
        menu.AddCheckBox(Config.Owners);

        menu.AddTitle("Overlay Updates");
        menu.SetColumns(4);
        menu.AddIntSlider(Config.UpdateInterval, "Update Interval", span: 4f, min: 1, max: 40);
    }
}
