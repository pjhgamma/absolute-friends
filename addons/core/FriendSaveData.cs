using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

internal static class FriendSaveData
{
    private const string EntrySeparator = "<svC>";

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

        sections.RemoveAll(section => section.StartsWith(sectionKey, StringComparison.Ordinal));
        save.unrecognizedSaveStrings?.RemoveAll(section => section.StartsWith(sectionKey, StringComparison.Ordinal));

        string content = string.Concat(entries.Select(entry => entry + EntrySeparator));

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
}
