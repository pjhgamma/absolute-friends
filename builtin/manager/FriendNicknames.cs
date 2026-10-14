using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Manager;

internal static class FriendNicknames
{
    internal const int MaxNicknameLength = 32;

    private const string SaveKey = "ABSOLUTEFRIENDS_NICKNAMES";

    private static readonly FriendSaveData.Store<State> _states = new(save => new(save));

    internal static void Register() => FriendDisplayNames.RegisterNameRule(NicknameOf);

    internal static void Unregister() => FriendDisplayNames.UnregisterNameRule(NicknameOf);

    private static string? NicknameOf(AbstractCreature abstractCreature) => _states.For(abstractCreature)?.Get(abstractCreature);

    private static string Normalize(string? value) => new([.. (value ?? string.Empty)
        .Trim()
        .Where(character => !char.IsControl(character))
        .Take(MaxNicknameLength)]);

    extension(AbstractCreature abstractCreature)
    {
        internal void SetNickname(string? value)
        {
            if (abstractCreature.IsPlayer || _states.For(abstractCreature) is not { } state)
            {
                return;
            }

            string nickname = Normalize(value);

            if (FriendSaveData.Assign(state.Nicknames, abstractCreature.SaveDataKey, nickname.Length == 0 ? null : nickname))
            {
                state.Write();
            }
        }
    }

    private sealed class State
    {
        internal readonly Dictionary<string, string> Nicknames = new(StringComparer.Ordinal);

        private readonly SaveState? _save;

        private ConditionalWeakTable<AbstractCreature, StrongBox<string?>> _cache = new();

        internal State(SaveState? save)
        {
            _save = save;

            foreach (string entry in FriendSaveData.ReadEntries(save, SaveKey))
            {
                int separator = entry.IndexOf('=');

                if (separator <= 0)
                {
                    continue;
                }

                try
                {
                    string nickname = Normalize(FriendSaveData.Decode(entry.Substring(separator + 1)));

                    if (nickname.Length > 0)
                    {
                        Nicknames[entry.Substring(0, separator)] = nickname;
                    }
                }
                catch (FormatException)
                {
                    continue;
                }
            }
        }

        internal string? Get(AbstractCreature creature) => _cache.GetValue(creature, subject =>
        {
            Nicknames.TryGetValue(subject.SaveDataKey, out string nickname);

            return new(nickname);
        }).Value;

        internal void Write()
        {
            _cache = new();

            FriendSaveData.WriteEntries(_save, SaveKey, Nicknames
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}={FriendSaveData.Encode(pair.Value)}"));
        }
    }
}
