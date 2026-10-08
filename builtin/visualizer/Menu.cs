using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using Menu.Remix.MixedUI;

namespace AbsoluteFriends.Visualizer;

internal static class Menu
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddTitle("Friend Display");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.FriendMarker);
        menu.AddCheckBox(Config.FriendMeter);
        menu.AddCheckBox(Config.BumpEffect);

        menu.AddTitle("Relationship Overlay");
        menu.SetColumns(3);
        menu.AddCheckBox(Config.Icon);
        menu.AddRadioButtonGroup(Config.IconStyle);
        menu.AddRadioButton("Creature Icon", "Uses creature artwork for relationship icons. Items keep their item icons.");
        menu.AddRadioButton("Symbol Icon", "Uses friendship symbols for creatures. Items keep their item icons.");
        menu.AddCheckBox(Config.Name);
        menu.AddCheckBox(Config.Link);

        menu.AddTitle("Shown Relationships");
        menu.SetColumns(5);
        menu.AddCheckBox(Config.FriendRelationship);
        OpSliderTick? shownFriendship = menu.AddIntSliderTick(Config.ShownFriendship, span: 3f);
        menu.AddCheckBox(Config.Owner);
        OpLabel[] friendships = menu.AddTickLabels(
            shownFriendship,
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

        menu.SetColumns(5);
        menu.AddIntSlider(Config.UpdateInterval, "Update Interval", span: 5f);
    }
}
