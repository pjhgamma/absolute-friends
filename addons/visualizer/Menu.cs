using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using Menu.Remix.MixedUI;

namespace AbsoluteFriends.Visualizer;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Friend Markers");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.FriendMarkers);
        menu.AddCheckBox(Config.FriendMeter);
        menu.AddCheckBox(Config.BumpEffects);

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
        OpLabel[] friendships = menu.AddTickLabels(
            menu.AddIntSliderTick(Config.ShownFriendship, "Friendship", span: 4f, min: 0, max: 3),
            "Tracking",
            "Friendly",
            "Neutral",
            "Indirect"
        );

        FriendshipType[] types = [FriendshipType.Tracked, FriendshipType.Friendly, FriendshipType.Neutral, FriendshipType.Indirect];

        for (int index = 0; index < friendships.Length; ++index)
        {
            friendships[index].color = Relationships.ColorOf(types[index]);
        }

        menu.AddCheckBox(Config.Owners);

        menu.AddTitle("Overlay Updates");
        menu.SetColumns(4);
        menu.AddIntSlider(Config.UpdateInterval, "Update Interval", span: 4f, min: 1, max: 40);
    }
}
