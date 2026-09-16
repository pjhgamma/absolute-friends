using RippleFriends.Options;

namespace RippleFriends.Items;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.SetColumns(4);
        menu.AddCheckBox(Config.Rock);
        menu.AddCheckBox(Config.Spear);
        menu.AddCheckBox(Config.ExplosiveSpear);
        menu.AddCheckBox(Config.ElectricSpear, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.HellSpear, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.PoisonSpear, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.LilyPuck, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.ScavengerBomb);
        menu.AddCheckBox(Config.SingularityBomb, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.FireEgg, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.SporePlant);
        menu.AddCheckBox(Config.Boomerang, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.Mushroom);
        menu.AddCheckBox(Config.FlareBomb);
        menu.AddCheckBox(Config.PuffBall);
        menu.AddCheckBox(Config.WaterNut);
        menu.AddCheckBox(Config.FirecrackerPlant);
        menu.AddCheckBox(Config.GraffitiBomb, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.DangleFruit, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.JellyFish);
        menu.AddCheckBox(Config.Pomegranate, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.Snail);
        menu.AddCheckBox(Config.TubeWorm);
        menu.AddCheckBox(Config.Frog, enabled: ModManager.Watcher);
    }
}
