using System.Runtime.CompilerServices;
using System.Text;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Manager;

internal static class FriendSaveData
{
    private const string SectionSeparator = "<dpB>";

    private const string EntrySeparator = "<dpC>";

    private const string LegacySectionSeparator = "<svB>";

    private const string LegacyEntrySeparator = "<svC>";

    private static readonly List<Action> _resets = [];

    internal static void Reset() => _resets.ForEach(reset => reset());

    internal static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    internal static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));

    internal static bool Assign<TKey>(Dictionary<TKey, string> values, TKey key, string? value)
    {
        if (value == null)
        {
            return values.Remove(key);
        }

        if (values.TryGetValue(key, out string previous) && previous == value)
        {
            return false;
        }

        values[key] = value;

        return true;
    }

    internal static IEnumerable<string> ReadEntries(SaveState? save, string sectionKey)
    {
        IEnumerable<(string Section, string Prefix, string Separator)> sections = [
            .. (save?.unrecognizedSaveStrings ?? []).Select(section => (section, sectionKey + LegacySectionSeparator, LegacyEntrySeparator)),
            .. (save?.deathPersistentSaveData?.unrecognizedSaveStrings ?? []).Select(section => (section, sectionKey + SectionSeparator, EntrySeparator))
        ];

        foreach (var (section, prefix, separator) in sections)
        {
            if (!section.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string entry in section.Substring(prefix.Length).Split([separator], StringSplitOptions.RemoveEmptyEntries))
            {
                yield return entry;
            }
        }
    }

    internal static void WriteEntries(SaveState? save, string sectionKey, IEnumerable<string> entries)
    {
        if (save?.deathPersistentSaveData?.unrecognizedSaveStrings is not { } sections)
        {
            return;
        }

        string content = string.Join(EntrySeparator, entries);

        sections.RemoveAll(section => section == sectionKey
            || section.StartsWith(sectionKey + SectionSeparator, StringComparison.Ordinal)
            || section.StartsWith(sectionKey + LegacySectionSeparator, StringComparison.Ordinal));
        save.unrecognizedSaveStrings?.RemoveAll(section => section.StartsWith(sectionKey + LegacySectionSeparator, StringComparison.Ordinal));

        if (content.Length > 0)
        {
            sections.Add(sectionKey + SectionSeparator + content);
        }
    }

    extension(AbstractCreature abstractCreature)
    {
        internal string SaveDataKey => abstractCreature is { IsPlayer: true, state: PlayerState playerState }
            ? $"P{playerState.playerNumber}"
            : $"C{abstractCreature.ID}";

        internal RainWorldGame? Game => abstractCreature.world?.game ?? RainWorldUtils.CurrentGame;
    }

    extension(RainWorldGame game)
    {
        internal SaveState? StorySaveState => game.session is StoryGameSession story ? story.saveState : null;
    }

    internal sealed class Store<T> where T : class
    {
        private readonly Func<SaveState?, T> _create;

        private ConditionalWeakTable<RainWorldGame, T> _states = new();

        internal Store(Func<SaveState?, T> create)
        {
            _create = create;
            _resets.Add(() => _states = new());
        }

        internal T For(RainWorldGame game) => _states.GetValue(game, game => _create(game.StorySaveState));

        internal T? For(AbstractCreature abstractCreature) => abstractCreature.Game is { } game ? For(game) : null;
    }
}
