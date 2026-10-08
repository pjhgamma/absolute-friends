using AbsoluteFriends.Core;
using AbsoluteFriends.Manager;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.FriendshipEditor;

internal sealed class LikeTab
{
    private const string TabId = Plugin.GUID + ".like";

    private const int MaxCreatures = 100;

    private readonly FriendTab _tab;

    private readonly RainWorldGame _game;

    private readonly OpComboBox _type;

    private readonly List<StatRow> _stats = [];

    private readonly ScrollList _list;

    private readonly List<CreatureRow> _creatureRows = [];

    private LikeTab(FriendTab tab, RainWorldGame game)
    {
        _tab = tab;
        _game = game;

        float width = tab.Size.x;
        float y = tab.Size.y - EditorUi.RowHeight;

        EditorUi.Label(tab, Translation.Of("Creature Type"), new Vector2(0f, y), StatRow.LabelWidth);
        _type = EditorUi.Combo(tab, Types(), Rules.All, new Vector2(StatRow.LabelWidth, y), width - StatRow.LabelWidth,
            Translation.Of("Choose the creature type to set rules for."));
        _type.OnValueUpdate += delegate { Reload(); };
        y -= StatRow.Height;

        foreach (Stat stat in (Stat[])[Stat.Like, Stat.TempLike, Stat.Know])
        {
            _stats.Add(new StatRow(tab, stat, y, SaveRule));
            y -= StatRow.Height;
        }

        _list = EditorUi.List(tab, Translation.Of("Loaded Creatures"), y);

        tab.FriendChanged += _ => Reload();
        tab.Shown += Reload;
        tab.Refresh += Refresh;
        tab.Update += () => EditorUi.KeepOpenComboOnTop([_type, .. _stats.Select(row => row.Mode)]);
    }

    private AbstractCreature? Target => _tab.Friend;

    private string TypeSubject => _type.value;

    internal static void Register() => FriendManager.RegisterTab(TabId, "Like", Build, friend => Config.LikeEditor.IsActive && (friend.IsPlayer || friend.IsTracked));

    internal static void Unregister() => FriendManager.UnregisterTab(TabId);

    private static IEnumerable<(string Key, string Name)> Types() => StaticWorld.creatureTemplates
        .Where(template => template is { virtualCreature: false } && template.type != CreatureTemplate.Type.Slugcat)
        .Select(template => template.type.value)
        .Distinct()
        .OrderBy(type => type, StringComparer.Ordinal)
        .Select(type => (type, type))
        .Prepend((Rules.All, Translation.Of("Every Creature Type")));

    private static void Build(FriendTab tab)
    {
        if (RainWorldUtils.CurrentGame is { } game)
        {
            _ = new LikeTab(tab, game);
        }
    }

    private RuleKey KeyOf(Stat stat) => RuleEffects.TypeKey(TypeSubject, Target!, stat);

    private void Reload()
    {
        if (Target == null || !_tab.IsActive)
        {
            return;
        }

        Rules rules = RuleStore.For(_game);

        foreach (StatRow row in _stats)
        {
            row.SetModes(TypeSubject == Rules.All);
            row.Load(rules.Exact(KeyOf(row.Stat)));
        }

        Refresh();
        _list.ScrollToTop();
    }

    private void SaveRule(StatRow row)
    {
        if (Target == null || row.EditedRule is { } rule && !rule.IsValid(row.Stat))
        {
            return;
        }

        RuleStore.For(_game).Set(KeyOf(row.Stat), row.EditedRule);
        RuleStore.Save(_game);
        RuleEffects.ApplyLikes(_game);
        Refresh();
    }

    private void Refresh()
    {
        if (Target is not { } target || !_tab.IsActive)
        {
            return;
        }

        Rules rules = RuleStore.For(_game);
        List<AbstractCreature> creatures = [.. TypeCreatures(target)
            .OrderByDescending(creature => creature.state?.socialMemory?.GetRelationship(target.ID) != null)
            .ThenByDescending(creature => creature.realizedCreature != null)
            .ThenBy(creature => creature.creatureTemplate.type.value, StringComparer.Ordinal)
            .ThenBy(creature => creature.ID.number)];

        foreach (StatRow row in _stats)
        {
            row.Refresh(rules.Resolve(KeyOf(row.Stat)));
        }

        ShowCreatures(creatures, target);
        _list.MarkDirty(0.5f);
    }

    private void ShowCreatures(List<AbstractCreature> creatures, AbstractCreature target)
    {
        int shown = Mathf.Min(creatures.Count, MaxCreatures);

        while (_creatureRows.Count < shown)
        {
            _creatureRows.Add(new CreatureRow(_tab, _list));
        }

        _list.ContentHeight = shown * CreatureRow.Height;

        float y = _list.ContentHeight;

        for (int index = 0; index < _creatureRows.Count; index++)
        {
            if (index < shown)
            {
                AbstractCreature creature = creatures[index];

                y -= CreatureRow.Height;
                _creatureRows[index].Show(creature, creature.state?.socialMemory?.GetRelationship(target.ID) is { } memory ? (memory.like, memory.tempLike, memory.know) : null, y);
            }
            else
            {
                _creatureRows[index].Hide();
            }
        }
    }

    private IEnumerable<AbstractCreature> TypeCreatures(AbstractCreature target) => RuleEffects.LoadedCreatures(_game).Values
        .Where(creature => !creature.IsPlayer && creature != target && (TypeSubject == Rules.All || creature.creatureTemplate.type.value == TypeSubject));
}
