using System.Runtime.CompilerServices;

namespace AbsoluteFriends.FriendshipEditor;

internal static class RuleStore
{
    private const string SectionKey = "ABSOLUTEFRIENDS_EDITOR<svB>";

    private static readonly ConditionalWeakTable<SaveState, Rules> _saves = new();

    private static readonly ConditionalWeakTable<RainWorldGame, Rules> _sessions = new();

    internal static Rules For(RainWorldGame game) => game.session is StoryGameSession { saveState: { } save }
        ? _saves.GetValue(save, Load)
        : _sessions.GetValue(game, _ => new());

    internal static void Save(RainWorldGame game)
    {
        if (game.session is not StoryGameSession { saveState.deathPersistentSaveData.unrecognizedSaveStrings: { } sections })
        {
            return;
        }

        Rules rules = For(game);

        sections.RemoveAll(section => section.StartsWith(SectionKey, StringComparison.Ordinal));

        if (!rules.IsEmpty)
        {
            sections.Add(SectionKey + rules.Serialize());
        }
    }

    private static Rules Load(SaveState save)
    {
        string? section = save.deathPersistentSaveData?.unrecognizedSaveStrings?.LastOrDefault(entry => entry.StartsWith(SectionKey, StringComparison.Ordinal));

        return section == null ? new() : Rules.Deserialize(section.Substring(SectionKey.Length));
    }
}
