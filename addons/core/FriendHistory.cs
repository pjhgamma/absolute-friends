using System.Globalization;
using System.Runtime.CompilerServices;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

internal static class FriendHistory
{
    private const string SaveKey = "ABSOLUTEFRIENDS_HISTORY<svB>";

    private const char FieldSeparator = '~';

    private static ConditionalWeakTable<RainWorldGame, State> _states = new();

    internal static void ResetHistory() => _states = new();

    internal static void CountSurvivedCycle(RainWorldGame game)
    {
        if (StateFor(game) is not { } state)
        {
            return;
        }

        bool changed = false;

        foreach (var abstractCreature in FriendUtils.ManageableFriends)
        {
            if (abstractCreature.IsPlayer || abstractCreature.state?.dead != false || !state.Records.TryGetValue(abstractCreature.SaveDataKey, out FriendRecord record))
            {
                continue;
            }

            record.Cycles++;
            changed = true;
        }

        if (changed)
        {
            state.Write();
        }
    }

    private static State? StateFor(RainWorldGame? game) => game?.StorySaveState is { } save
        ? _states.GetValue(game, _ => new(save))
        : null;

    extension(AbstractCreature abstractCreature)
    {
        internal FriendRecord? MetRecord => StateFor(abstractCreature.Game) is { } state && state.Records.TryGetValue(abstractCreature.SaveDataKey, out FriendRecord record)
            ? record
            : null;

        internal void RecordMeeting()
        {
            if (abstractCreature.IsPlayer || abstractCreature.Game is not { StorySaveState: { } save } game || StateFor(game) is not { } state)
            {
                return;
            }

            string key = abstractCreature.SaveDataKey;

            if (state.Records.ContainsKey(key))
            {
                return;
            }

            state.Records[key] = new(save.cycleNumber, abstractCreature.Room?.name ?? string.Empty, 0);
            state.Write();
        }

        internal void ForgetMeeting()
        {
            if (StateFor(abstractCreature.Game) is { } state && state.Records.Remove(abstractCreature.SaveDataKey))
            {
                state.Write();
            }
        }
    }

    internal sealed class FriendRecord(int cycle, string room, int cycles)
    {
        internal int Cycle { get; } = cycle;

        internal string Room { get; } = room;

        internal int Cycles { get; set; } = cycles;
    }

    private sealed class State
    {
        internal readonly Dictionary<string, FriendRecord> Records = new(StringComparer.Ordinal);

        private readonly SaveState _save;

        internal State(SaveState save)
        {
            _save = save;

            foreach (string entry in FriendSaveData.ReadEntries(save, SaveKey))
            {
                int separator = entry.IndexOf('=');
                string[] fields = separator > 0 ? entry.Substring(separator + 1).Split(FieldSeparator) : [];

                if (
                    fields.Length != 3
                    || !int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cycle)
                    || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cycles)
                )
                {
                    continue;
                }

                Records[entry.Substring(0, separator)] = new(cycle, fields[1], cycles);
            }
        }

        internal void Write() => FriendSaveData.WriteEntries(_save, SaveKey, Records
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => string.Join(FieldSeparator.ToString(), [
                $"{pair.Key}={pair.Value.Cycle.ToString(CultureInfo.InvariantCulture)}",
                pair.Value.Room,
                pair.Value.Cycles.ToString(CultureInfo.InvariantCulture)
            ])));
    }
}
