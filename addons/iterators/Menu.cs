using AbsoluteFriends.Options;

namespace AbsoluteFriends.Iterators;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.SetColumns(4);
        menu.AddCheckBox(Config.Moon);
        menu.AddCheckBox(Config.MoonNeuron);
        menu.AddCheckBox(Config.Pebbles);
        menu.AddCheckBox(Config.PebblesPearl, enabled: ModManager.MSC);
        menu.AddCheckBox(Config.Prince, enabled: ModManager.Watcher);
        menu.AddCheckBox(Config.Overseer);
        menu.AddCheckBox(Config.Mark);
    }
}
