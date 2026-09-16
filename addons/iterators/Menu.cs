using RippleFriends.Options;

namespace RippleFriends.Iterators;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.SetColumns(4);
        menu.AddCheckBox(Config.Moon);
        menu.AddCheckBox(Config.MoonNeuron);
        menu.AddCheckBox(Config.Pebbles);
        menu.AddCheckBox(Config.PebblesPearl, enabled: ModManager.MSC);
    }
}
