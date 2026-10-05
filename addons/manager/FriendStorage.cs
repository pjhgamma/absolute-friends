namespace AbsoluteFriends.Manager;

public static class FriendStorage
{
    private const string SaveKey = "ABSOLUTEFRIENDS_STORAGE<svB>";

    private static readonly FriendSaveData.Store<State> _states = new(save => new(save));

    extension(AbstractCreature abstractCreature)
    {
        public string? GetFriendData(string key) => !string.IsNullOrEmpty(key) && _states.For(abstractCreature) is { } state
            && state.Values.TryGetValue((abstractCreature.SaveDataKey, key), out string value)
            ? value
            : null;

        public void SetFriendData(string key, string? value)
        {
            if (string.IsNullOrEmpty(key) || _states.For(abstractCreature) is not { } state)
            {
                return;
            }

            (string Creature, string Key) entry = (abstractCreature.SaveDataKey, key);

            if (value == null)
            {
                if (state.Values.Remove(entry))
                {
                    state.Write();
                }
            }
            else if (!state.Values.TryGetValue(entry, out string previous) || previous != value)
            {
                state.Values[entry] = value;
                state.Write();
            }
        }

        internal void ClearFriendData()
        {
            if (_states.For(abstractCreature) is not { } state)
            {
                return;
            }

            string creature = abstractCreature.SaveDataKey;

            if (state.Values.Keys.Where(entry => entry.Creature == creature).ToList() is { Count: > 0 } entries)
            {
                entries.ForEach(entry => state.Values.Remove(entry));
                state.Write();
            }
        }
    }

    private sealed class State
    {
        internal readonly Dictionary<(string Creature, string Key), string> Values = [];

        private readonly SaveState? _save;

        internal State(SaveState? save)
        {
            _save = save;

            foreach (string entry in FriendSaveData.ReadEntries(save, SaveKey))
            {
                string[] parts = entry.Split('=');

                if (parts.Length != 3 || parts[0].Length == 0)
                {
                    continue;
                }

                try
                {
                    Values[(parts[0], FriendSaveData.Decode(parts[1]))] = FriendSaveData.Decode(parts[2]);
                }
                catch (FormatException)
                {
                    continue;
                }
            }
        }

        internal void Write() => FriendSaveData.WriteEntries(_save, SaveKey, Values
            .OrderBy(pair => pair.Key.Creature, StringComparer.Ordinal)
            .ThenBy(pair => pair.Key.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key.Creature}={FriendSaveData.Encode(pair.Key.Key)}={FriendSaveData.Encode(pair.Value)}"));
    }
}
