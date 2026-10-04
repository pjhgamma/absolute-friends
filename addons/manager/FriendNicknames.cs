using System.Runtime.CompilerServices;
using System.Text;
using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Manager;

internal static class FriendNicknames
{
    internal const int MaxNicknameLength = 20;

    private const string SaveKey = "ABSOLUTEFRIENDS_NICKNAMES<svB>";

    private static ConditionalWeakTable<RainWorldGame, State> _states = new();

    internal static void Register() => FriendDisplayNames.RegisterNameRule(NicknameOf);

    internal static void Unregister() => FriendDisplayNames.UnregisterNameRule(NicknameOf);

    internal static void ResetNicknames() => _states = new();

    private static string? NicknameOf(AbstractCreature abstractCreature) => StateFor(abstractCreature)?.Get(abstractCreature);

    private static string Normalize(string? value) => new([.. (value ?? string.Empty)
        .Trim()
        .Where(character => !char.IsControl(character))
        .Take(MaxNicknameLength)]);

    private static State? StateFor(AbstractCreature abstractCreature) => abstractCreature.Game is { } game
        ? _states.GetValue(game, game => new(game.StorySaveState))
        : null;

    extension(AbstractCreature abstractCreature)
    {
        internal void SetNickname(string? value)
        {
            if (abstractCreature.IsPlayer || StateFor(abstractCreature) is not { } state)
            {
                return;
            }

            string key = abstractCreature.SaveDataKey;
            string nickname = Normalize(value);

            if (nickname.Length == 0)
            {
                if (state.Nicknames.Remove(key))
                {
                    state.Write();
                }
            }
            else if (!state.Nicknames.TryGetValue(key, out string previous) || previous != nickname)
            {
                state.Nicknames[key] = nickname;
                state.Write();
            }
        }
    }

    private sealed class State
    {
        internal readonly Dictionary<string, string> Nicknames = new(StringComparer.Ordinal);

        private readonly SaveState? _save;

        private ConditionalWeakTable<AbstractCreature, CachedNickname> _cache = new();

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
                    string nickname = Normalize(Encoding.UTF8.GetString(Convert.FromBase64String(entry.Substring(separator + 1))));

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
                .Select(pair => $"{pair.Key}={Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value))}"));
        }
    }

    private sealed class CachedNickname(string? value)
    {
        internal string? Value { get; } = value;
    }
}
