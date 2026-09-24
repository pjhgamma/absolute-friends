using AbsoluteFriends.Options;

namespace AbsoluteFriends.Progression;

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
        menu.AddCheckBox(Config.Passage);
        menu.AddCheckBox(Config.TempleGuard);

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
