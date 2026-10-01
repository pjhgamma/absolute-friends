using AbsoluteFriends.Options;

namespace AbsoluteFriends.Pilgrimage;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Thresholds");
        menu.SetColumns(4);
        menu.AddCheckBox(Config.Gate);
        menu.AddFloatSlider(Config.GateTime, span: 3f, max: 5f);
        menu.AddCheckBox(Config.GateForce);
        menu.AddFloatSlider(Config.GateForceTime, span: 3f, max: 5f);
        menu.AddCheckBox(Config.TempleGuard);
        menu.AddCheckBox(Config.Ascension);
    }
}
