using AbsoluteFriends.Manager;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class ReputationTab
{
    private const string TabId = Plugin.GUID + ".reputation";

    private readonly FriendTab _tab;

    private readonly RainWorldGame _game;

    private readonly CreatureCommunities _communities;

    private readonly OpComboBox _community;

    private readonly OpComboBox? _region;

    private readonly StatRow _reputation;

    private readonly ScrollList _list;

    private readonly List<ReputationRow> _rows = [];

    private ReputationTab(FriendTab tab, RainWorldGame game)
    {
        _tab = tab;
        _game = game;
        _communities = game.session.creatureCommunities;

        float width = tab.Size.x;
        float y = tab.Size.y - EditorUi.RowHeight;

        EditorUi.Label(tab, Translation.Of("Community"), new Vector2(0f, y), StatRow.LabelWidth);
        _community = EditorUi.Combo(tab, Communities().Prepend((Rules.All, Translation.Of("Every Community"))), Rules.All, new Vector2(StatRow.LabelWidth, y), width - StatRow.LabelWidth,
            Translation.Of("Choose the community to set rules for."));
        _community.OnValueUpdate += delegate { Reload(); };
        y -= StatRow.Height;

        if (game.IsStorySession)
        {
            EditorUi.Label(tab, Translation.Of("Region"), new Vector2(0f, y), StatRow.LabelWidth);
            _region = EditorUi.Combo(tab, Regions().Prepend((Rules.All, Translation.Of("Every Region"))), Rules.All, new Vector2(StatRow.LabelWidth, y), width - StatRow.LabelWidth,
                Translation.Of("Choose the region to set rules for."));
            _region.OnValueUpdate += delegate { Reload(); };
            y -= StatRow.Height;
        }

        _reputation = new StatRow(tab, Stat.Reputation, y, SaveRule);
        y -= StatRow.Height;
        _list = EditorUi.List(tab, Translation.Of(game.IsStorySession ? "Regional Reputation" : "Community Reputation"), y);

        foreach ((string key, string name) in game.IsStorySession ? Regions() : Communities())
        {
            _rows.Add(new ReputationRow(tab, _list, key, name));
        }

        _list.ContentHeight = _rows.Count * ReputationRow.Height;

        tab.FriendChanged += _ => Reload();
        tab.Shown += Reload;
        tab.Refresh += Refresh;
        tab.Update += () => EditorUi.KeepOpenComboOnTop(_region == null ? [_community, _reputation.Mode] : [_community, _region, _reputation.Mode]);
    }

    private int PlayerNumber => _tab.Friend?.state is PlayerState state ? state.playerNumber : 0;

    private int ReputationPlayer => _game.IsStorySession ? 0 : PlayerNumber;

    private CreatureCommunities.CommunityID ShownCommunity => _community.value == Rules.All ? CreatureCommunities.CommunityID.All : new(_community.value);

    private RuleKey SelectedKey => RuleEffects.ReputationKey(_game, _community.value, _region?.value ?? Rules.All, ReputationPlayer);

    internal static void Register() => FriendManager.RegisterTab(TabId, "Reputation", Build, IsVisibleFor);

    internal static void Unregister() => FriendManager.UnregisterTab(TabId);

    private static bool IsVisibleFor(AbstractCreature friend) =>
        Config.ReputationEditor.IsActive
        && friend.IsPlayer
        && friend.state is PlayerState state
        && RainWorldUtils.CurrentGame is { session.creatureCommunities: { } communities } game
        && (game.IsStorySession ? state.playerNumber == 0 : state.playerNumber < communities.playerOpinions.GetLength(2));

    private static void Build(FriendTab tab)
    {
        if (RainWorldUtils.CurrentGame is { session.creatureCommunities: not null } game)
        {
            _ = new ReputationTab(tab, game);
        }
    }

    private IEnumerable<(string Key, string Name)> Communities()
    {
        foreach (string name in CreatureCommunities.CommunityID.values.entries)
        {
            if (name != CreatureCommunities.CommunityID.None.value && new CreatureCommunities.CommunityID(name).Index - 1 < _communities.playerOpinions.GetLength(0))
            {
                yield return (name, name);
            }
        }
    }

    private IEnumerable<(string Key, string Name)> Regions()
    {
        string[] regions = _game.rainWorld.progression.regionNames;
        SlugcatStats.Name? slugcat = (_game.session as StoryGameSession)?.saveStateNumber;

        yield return (RuleEffects.GlobalRegion, "Global");

        for (int region = 0; region < regions.Length && region + 1 < _communities.playerOpinions.GetLength(1); region++)
        {
            yield return (regions[region], $"{regions[region]}  {Translation.Of(Region.GetRegionFullName(regions[region], slugcat))}");
        }
    }

    private int RegionIndex(string key) => Array.IndexOf(_game.rainWorld.progression.regionNames, key);

    private float Saved(CreatureCommunities.CommunityID community, int region) => _communities.playerOpinions[community.Index - 1, region + 1, ReputationPlayer];

    private void Reload()
    {
        if (_tab.Friend == null || !_tab.IsActive)
        {
            return;
        }

        RuleKey key = SelectedKey;
        Rule? rule = RuleStore.For(_game).Exact(key);

        _reputation.SetModes(key.Subject == Rules.All && key.Region == Rules.All);
        _reputation.Load(rule);
        Refresh();
    }

    private void SaveRule(StatRow row)
    {
        if (_tab.Friend == null || row.EditedRule is { } rule && !rule.IsValid(row.Stat))
        {
            return;
        }

        RuleStore.For(_game).Set(SelectedKey, row.EditedRule);
        RuleStore.Save(_game);
        RuleEffects.ApplyReputation(_game);
        Refresh();
    }

    private void Refresh()
    {
        if (_tab.Friend == null || !_tab.IsActive)
        {
            return;
        }

        _reputation.Refresh(RuleStore.For(_game).Resolve(SelectedKey));

        float y = _list.ContentHeight;

        foreach (ReputationRow row in _rows)
        {
            y -= ReputationRow.Height;

            if (_game.IsStorySession)
            {
                row.Show(y, Saved(ShownCommunity, RegionIndex(row.Key)));
            }
            else
            {
                CreatureCommunities.CommunityID community = new(row.Key);

                row.Show(y, Saved(community, -1));
            }
        }

        _list.MarkDirty(0.5f);
    }
}
