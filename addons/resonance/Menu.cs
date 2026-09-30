using AbsoluteFriends.Options;

namespace AbsoluteFriends.Resonance;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Resonance");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.ResonanceGate);
        menu.AddCheckBox(Config.ResonanceRoom);
        menu.AddCheckBox(Config.ResonanceGrab);
        menu.SetColumns(2);
        menu.AddCheckBox(Config.ResonanceWarp);
        menu.AddCheckBox(Config.ResonanceMend);
        menu.SetColumns(4);
        menu.AddCheckBox(Config.ResonanceCost);
        menu.AddFloatSlider(Config.ResonanceCostRatio, span: 3f);
        menu.AddCheckBox(Config.ResonanceAftershock);
        menu.AddFloatSlider(Config.ResonanceAftershockRatio, span: 3f);
        menu.AddCheckBox(Config.ResonanceEffect);
    }
}
