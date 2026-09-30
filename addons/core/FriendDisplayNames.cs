using System.Runtime.CompilerServices;
using System.Text;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core;

public static class FriendDisplayNames
{
    private const string SaveKey = "ABSOLUTEFRIENDS_NICKNAMES<svB>";

    internal const int MaxNicknameLength = 20;

    private static ConditionalWeakTable<RainWorldGame, State> _states = new();

    internal static void ResetNicknames() => _states = new();

    private static string Normalize(string? value) => new([.. (value ?? string.Empty)
        .Trim()
        .Where(character => !char.IsControl(character))
        .Take(MaxNicknameLength)]);

    private static State? StateFor(AbstractCreature abstractCreature) => abstractCreature.Game is { } game
        ? _states.GetValue(game, game => new(game.StorySaveState))
        : null;

    extension(AbstractCreature abstractCreature)
    {
        public string PlayerName => JollyCoop.JollyCustom.GetPlayerName(
            abstractCreature.state is PlayerState state ? state.playerNumber : 0
        );

        public string FriendName => abstractCreature.IsPlayer
            ? abstractCreature.PlayerName
            : StateFor(abstractCreature)?.Get(abstractCreature) ?? abstractCreature.DefaultName;

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

    extension(AbstractPhysicalObject? abstractPhysicalObject)
    {
        public string DefaultName => abstractPhysicalObject switch
        {
            AbstractOwner owner => owner.DisplayName,
            AbstractCreature { creatureTemplate.name: { } name } creature => $"{name} {creature.ID.number}",
            { type: { } type } => $"{type} {abstractPhysicalObject.ID.number}",
            _ => string.Empty,
        };
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

        internal string? Get(AbstractCreature creature)
        {
            if (_cache.TryGetValue(creature, out CachedNickname cached))
            {
                return cached.Value;
            }

            Nicknames.TryGetValue(creature.SaveDataKey, out string nickname);
            _cache.Add(creature, new CachedNickname(nickname));

            return nickname;
        }

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
