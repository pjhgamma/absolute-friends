using AbsoluteFriends.Manager;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class ReputationView : EditorView
{
    private readonly CreatureCommunities _communities;

    private readonly OpComboBox _community;

    private readonly OpComboBox? _region;

    private readonly StatRow _reputation;

    private readonly RuleList _rules;

    private readonly ScrollList _list;

    private readonly List<ReputationRow> _rows = [];

    internal ReputationView(FriendTab tab, RainWorldGame game, CreatureCommunities communities, float y) : base(tab, game)
    {
        float width = tab.Size.x;

        _communities = communities;

        EditorUi.Label(Add, Translation.Of("Community"), new Vector2(0f, y), StatRow.LabelWidth);
        _community = EditorUi.Combo(Add, Communities().Prepend((Rules.All, Translation.Of("Every Community"))), Rules.All, new Vector2(StatRow.LabelWidth, y), width - StatRow.LabelWidth,
            Translation.Of("Choose the community to set rules for."));
        _community.OnValueUpdate += delegate { Reload(); };
        y -= StatRow.Height;

        if (game.IsStorySession)
        {
            EditorUi.Label(Add, Translation.Of("Region"), new Vector2(0f, y), StatRow.LabelWidth);
            _region = EditorUi.Combo(Add, Regions().Prepend((Rules.All, Translation.Of("Every Region"))), Rules.All, new Vector2(StatRow.LabelWidth, y), width - StatRow.LabelWidth,
                Translation.Of("Choose the region to set rules for."));
            _region.OnValueUpdate += delegate { Reload(); };
            y -= StatRow.Height;
        }

        _reputation = new StatRow(this, Stat.Reputation, y);
        y -= StatRow.Height;
        _rules = new RuleList(this, y, ApplyRule, RemoveRule, SelectRule);
        y -= RuleList.Height;
        _list = EditorUi.List(Add, width, Translation.Of(game.IsStorySession ? "Regional Reputation" : "Community Reputation"), y);

        foreach ((string key, string name) in game.IsStorySession ? Regions() : Communities())
        {
            _rows.Add(new ReputationRow(tab, _list, key, name));
        }

        _list.ContentHeight = _rows.Count * ReputationRow.Height;
    }

    internal override string Title => "Reputation";

    private int ReputationPlayer => Game.IsStorySession ? 0 : PlayerNumber(Target);

    private CreatureCommunities.CommunityID ShownCommunity => _community.value == Rules.All ? CreatureCommunities.CommunityID.All : new(_community.value);

    private RuleKey SelectedKey => RuleEffects.ReputationKey(Game, _community.value, _region?.value ?? Rules.All, ReputationPlayer);

    internal static bool IsAvailableFor(AbstractCreature friend) =>
        Config.ReputationEditor.IsActive
        && friend.IsPlayer
        && friend.state is PlayerState state
        && RainWorldUtils.CurrentGame is { session.creatureCommunities: { } communities } game
        && (game.IsStorySession ? state.playerNumber == 0 : state.playerNumber < communities.playerOpinions.GetLength(2));

    private static int PlayerNumber(AbstractCreature? friend) => friend?.state is PlayerState state ? state.playerNumber : 0;

    internal override bool IsVisibleFor(AbstractCreature friend) => IsAvailableFor(friend);

    internal override bool Owns(RuleKey key, AbstractCreature friend) =>
        key.Stat == Stat.Reputation && key.Target == RuleEffects.ReputationKey(Game, Rules.All, Rules.All, Game.IsStorySession ? 0 : PlayerNumber(friend)).Target;

    internal override void Reload() => Load(null);

    internal override void Refresh()
    {
        if (Target == null)
        {
            return;
        }

        _reputation.Refresh(RuleStore.For(Game).Resolve(SelectedKey));

        float y = _list.ContentHeight;

        foreach (ReputationRow row in _rows)
        {
            y -= ReputationRow.Height;

            if (Game.IsStorySession)
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
        _rules.Show(ConfiguredRules());
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
        string[] regions = Game.rainWorld.progression.regionNames;
        SlugcatStats.Name? slugcat = (Game.session as StoryGameSession)?.saveStateNumber;

        yield return (RuleEffects.GlobalRegion, "Global");

        for (int region = 0; region < regions.Length && region + 1 < _communities.playerOpinions.GetLength(1); region++)
        {
            yield return (regions[region], $"{regions[region]}  {Translation.Of(Region.GetRegionFullName(regions[region], slugcat))}");
        }
    }

    private int RegionIndex(string key) => Array.IndexOf(Game.rainWorld.progression.regionNames, key);

    private float Saved(CreatureCommunities.CommunityID community, int region) => _communities.playerOpinions[community.Index - 1, region + 1, ReputationPlayer];

    private static string CommunityName(string community) => community == Rules.All ? Translation.Of("Every Community") : community;

    private static string RegionName(string region) => region switch
    {
        Rules.All => Translation.Of("Every Region"),
        RuleEffects.GlobalRegion => "Global",
        _ => region,
    };

    private void Load(RuleKey? selection)
    {
        if (Target == null)
        {
            return;
        }

        List<(RuleKey Key, string Text)> configured = [.. ConfiguredRules()];
        RuleKey? shown = selection ?? _rules.Choose([.. configured.Select(rule => rule.Key)], [SelectedKey]);

        if (shown is { } chosen)
        {
            EditorUi.ShowValue(_community, chosen.Subject);

            if (_region != null)
            {
                EditorUi.ShowValue(_region, chosen.Region);
            }
        }

        RuleKey key = SelectedKey;

        _reputation.SetModes(key.Subject == Rules.All && key.Region == Rules.All);
        _reputation.Load(RuleStore.For(Game).Exact(key));
        Refresh();
        _rules.Show(configured, shown);
    }

    private IEnumerable<(RuleKey Key, string Text)> ConfiguredRules() => Target is not { } target ? [] : RuleStore.For(Game)
        .Matching(key => Owns(key, target))
        .OrderBy(rule => rule.Key.Subject != Rules.All)
        .ThenBy(rule => rule.Key.Subject, StringComparer.Ordinal)
        .ThenBy(rule => rule.Key.Region != Rules.All)
        .ThenBy(rule => rule.Key.Region, StringComparer.Ordinal)
        .Select(rule => (rule.Key, Game.IsStorySession
            ? $"{CommunityName(rule.Key.Subject)} - {RegionName(rule.Key.Region)}: {EditorUi.RuleText(rule.Rule)}"
            : $"{CommunityName(rule.Key.Subject)}: {EditorUi.RuleText(rule.Rule)}"));

    private void ApplyRule()
    {
        if (Target == null || _reputation.EditedRule is { } rule && !rule.IsValid(Stat.Reputation))
        {
            return;
        }

        RuleStore.For(Game).Set(SelectedKey, _reputation.EditedRule);
        RuleStore.Save(Game);
        RuleEffects.ApplyReputation(Game);
        Load(null);
    }

    private void RemoveRule(RuleKey key)
    {
        RuleStore.For(Game).Set(key, null);
        RuleStore.Save(Game);
        Load(null);
    }

    private void SelectRule(RuleKey key) => Load(key);
}
