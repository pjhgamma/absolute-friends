using AbsoluteFriends.Core;
using AbsoluteFriends.Manager;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal abstract class CreatureView : EditorView
{
    private const int MaxCreatures = 100;

    private readonly OpComboBox _type;

    private readonly List<StatRow> _stats = [];

    private readonly RuleList _rules;

    private readonly ScrollList _list;

    private readonly List<CreatureRow> _creatureRows = [];

    protected CreatureView(FriendTab tab, RainWorldGame game, float y, params Stat[] stats) : base(tab, game)
    {
        float width = tab.Size.x;

        EditorUi.Label(Add, Translation.Of("Creature Type"), new Vector2(0f, y), StatRow.LabelWidth);
        _type = EditorUi.Combo(Add, Types(), Rules.All, new Vector2(StatRow.LabelWidth, y), width - StatRow.LabelWidth,
            Translation.Of("Choose the creature type to set rules for."));
        _type.OnValueUpdate += delegate { Reload(); };
        y -= StatRow.Height;

        foreach (Stat stat in stats)
        {
            _stats.Add(new StatRow(this, stat, y));
            y -= StatRow.Height;
        }

        _rules = new RuleList(this, y, ApplyRules, RemoveRule, SelectRule);
        y -= RuleList.Height;
        _list = EditorUi.List(Add, width, Translation.Of("Loaded Creatures"), y);
    }

    private string TypeSubject => _type.value;

    protected static bool IsFriendTarget(AbstractCreature friend) => friend.IsPlayer || friend.IsTracked;

    private static IEnumerable<(string Key, string Name)> Types() => StaticWorld.creatureTemplates
        .Where(template => template is { virtualCreature: false } && template.type != CreatureTemplate.Type.Slugcat)
        .Select(template => template.type.value)
        .Distinct()
        .OrderBy(type => type, StringComparer.Ordinal)
        .Select(type => (type, type))
        .Prepend((Rules.All, Translation.Of("Every Creature Type")));

    internal override bool Owns(RuleKey key, AbstractCreature friend) => _stats.Any(row => row.Stat == key.Stat) && key.Target == RuleEffects.TargetKey(friend);

    internal override void Reload() => Load(null);

    internal override void Refresh()
    {
        if (Target is not { } target)
        {
            return;
        }

        Rules rules = RuleStore.For(Game);

        foreach (StatRow row in _stats)
        {
            row.Refresh(rules.Resolve(KeyOf(row.Stat)));
        }

        ShowCreatures([.. TypeCreatures(target)
            .OrderByDescending(creature => Knows(creature, target))
            .ThenByDescending(creature => creature.realizedCreature != null)
            .ThenBy(creature => creature.creatureTemplate.type.value, StringComparer.Ordinal)
            .ThenBy(creature => creature.ID.number)], target);
        _list.MarkDirty(0.5f);
        _rules.Show(ConfiguredRules(target));
    }

    protected abstract bool Knows(AbstractCreature creature, AbstractCreature target);

    protected abstract void ShowRow(CreatureRow row, AbstractCreature creature, AbstractCreature target, float y);

    protected virtual void Apply()
    {
    }

    private RuleKey KeyOf(Stat stat) => RuleEffects.TypeKey(TypeSubject, Target!, stat);

    private static string TypeName(string type) => type == Rules.All ? Translation.Of("Every Creature Type") : type;

    private void Load(RuleKey? selection)
    {
        if (Target is not { } target)
        {
            return;
        }

        Rules rules = RuleStore.For(Game);
        List<(RuleKey Key, string Text)> configured = [.. ConfiguredRules(target)];
        RuleKey? shown = selection ?? _rules.Choose([.. configured.Select(rule => rule.Key)], _stats.Select(row => KeyOf(row.Stat)));

        if (shown is { } key)
        {
            EditorUi.ShowValue(_type, key.Subject);
        }

        foreach (StatRow row in _stats)
        {
            row.SetModes(TypeSubject == Rules.All);
            row.Load(rules.Exact(KeyOf(row.Stat)));
        }

        Refresh();
        _rules.Show(configured, shown);
        _list.ScrollToTop();
    }

    private IEnumerable<(RuleKey Key, string Text)> ConfiguredRules(AbstractCreature target) => RuleStore.For(Game)
        .Matching(key => Owns(key, target))
        .OrderBy(rule => rule.Key.Subject != Rules.All)
        .ThenBy(rule => rule.Key.Subject, StringComparer.Ordinal)
        .ThenBy(rule => rule.Key.Stat)
        .Select(rule => (rule.Key, _stats.Count > 1
            ? $"{TypeName(rule.Key.Subject)} - {EditorUi.StatName(rule.Key.Stat)}: {EditorUi.RuleText(rule.Rule)}"
            : $"{TypeName(rule.Key.Subject)}: {EditorUi.RuleText(rule.Rule)}"));

    private void ApplyRules()
    {
        if (Target == null)
        {
            return;
        }

        Rules rules = RuleStore.For(Game);

        foreach (StatRow row in _stats)
        {
            if (row.EditedRule is not { } rule || rule.IsValid(row.Stat))
            {
                rules.Set(KeyOf(row.Stat), row.EditedRule);
            }
        }

        RuleStore.Save(Game);
        Apply();
        Load(null);
    }

    private void RemoveRule(RuleKey key)
    {
        RuleStore.For(Game).Set(key, null);
        RuleStore.Save(Game);
        Apply();
        Load(null);
    }

    private void SelectRule(RuleKey key) => Load(key);

    private void ShowCreatures(List<AbstractCreature> creatures, AbstractCreature target)
    {
        int shown = Mathf.Min(creatures.Count, MaxCreatures);

        while (_creatureRows.Count < shown)
        {
            _creatureRows.Add(new CreatureRow(Tab, _list));
        }

        _list.ContentHeight = shown * CreatureRow.Height;

        float y = _list.ContentHeight;

        for (int index = 0; index < _creatureRows.Count; index++)
        {
            if (index < shown)
            {
                y -= CreatureRow.Height;
                ShowRow(_creatureRows[index], creatures[index], target, y);
            }
            else
            {
                _creatureRows[index].Hide();
            }
        }
    }

    private IEnumerable<AbstractCreature> TypeCreatures(AbstractCreature target) => RuleEffects.LoadedCreatures(Game).Values
        .Where(creature => !creature.IsPlayer && creature != target && (TypeSubject == Rules.All || creature.creatureTemplate.type.value == TypeSubject));
}
