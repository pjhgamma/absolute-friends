using AbsoluteFriends.Options;

namespace AbsoluteFriends.Creatures;

internal static class Menu
{
    internal static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Lizards");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.LizardBite);
        menu.AddCheckBox(Config.LizardTongue);
        menu.AddCheckBox(Config.LizardSpit);
        menu.AddCheckBox(Config.LizardBeam, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.LizardBlizzard, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.LizardPoison, enabled: ModManager.Watcher);

        menu.AddTitle("Scavengers");
        menu.SetColumns(2);
        menu.AddCheckBox(Config.ScavengerShelter);
        menu.AddCheckBox(Config.ScavengerTemplar, enabled: ModManager.Watcher);
    }
}
