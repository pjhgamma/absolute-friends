using AbsoluteFriends.Options;

namespace AbsoluteFriends.General;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.SetColumns(5);
        menu.AddCheckBox(Config.Collision);
        menu.AddCheckBox(Config.Violence);
        menu.AddCheckBox(Config.Explosion);
        menu.AddCheckBox(Config.Fear);
        menu.AddCheckBox(Config.Stealing);
        menu.SetColumns(4);
        menu.AddCheckBox(Config.Deaf);
        menu.AddFloatSlider(Config.DeafRatio, span: 3f);
        menu.AddCheckBox(Config.Blind);
        menu.AddFloatSlider(Config.BlindRatio, span: 3f);
        menu.AddCheckBox(Config.Hypothermia, enabled: ModManager.HypothermiaModule);
        menu.AddFloatSlider(Config.HypothermiaRatio, span: 3f, enabled: ModManager.HypothermiaModule);
        menu.AddCheckBox(Config.Forgiveness);
        menu.AddFloatSlider(Config.ForgivenessRatio, span: 3f);
    }
}
