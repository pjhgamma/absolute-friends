using System.Runtime.CompilerServices;
using System.Text;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Manager;

internal static class FriendSaveData
{
    private const string EntrySeparator = "<svC>";

    private static readonly List<Action> _resets = [];

    internal static void Reset() => _resets.ForEach(reset => reset());

    internal static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    internal static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));

    internal static IEnumerable<string> ReadEntries(SaveState? save, string sectionKey)
    {
        List<string> sections = [.. save?.unrecognizedSaveStrings ?? [], .. save?.deathPersistentSaveData?.unrecognizedSaveStrings ?? []];

        foreach (string section in sections)
        {
            if (!section.StartsWith(sectionKey, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string entry in section.Substring(sectionKey.Length).Split([EntrySeparator], StringSplitOptions.RemoveEmptyEntries))
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

        string content = string.Concat(entries.Select(entry => entry + EntrySeparator));

        sections.RemoveAll(section => section.StartsWith(sectionKey, StringComparison.Ordinal));
        save.unrecognizedSaveStrings?.RemoveAll(section => section.StartsWith(sectionKey, StringComparison.Ordinal));

        if (content.Length > 0)
        {
            sections.Add(sectionKey + content);
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
