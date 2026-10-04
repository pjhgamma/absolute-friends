using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Manager;

internal static class FriendTracking
{
    private const int ScanInterval = 10;

    private static ConditionalWeakTable<RainWorldGame, TrackingChoices> _choices = new();

    private static ConditionalWeakTable<RainWorldGame, HashSet<AbstractCreature>> _metFriends = new();

    private static int _scanTimer;

    internal static void Register() => FriendUtils.RegisterTrackingRule(IsIncluded);

    internal static void Unregister() => FriendUtils.UnregisterTrackingRule(IsIncluded);

    internal static void ResetTracking()
    {
        _choices = new();
        _metFriends = new();
    }

    internal static void MeetNewFriends(RainWorldGame game)
    {
        if (++_scanTimer < ScanInterval)
        {
            return;
        }

        _scanTimer = 0;

        HashSet<AbstractCreature> metFriends = _metFriends.GetOrCreateValue(game);

        foreach (var abstractCreature in FriendUtils.TrackingCandidates)
        {
            if (abstractCreature.IsPlayer || !metFriends.Add(abstractCreature))
            {
                continue;
            }

            abstractCreature.ApplyDefaultTracking();
            abstractCreature.RecordMeeting();
        }
    }

    private static bool IsIncluded(AbstractCreature abstractCreature) => !abstractCreature.IsExcluded;

    private static TrackingChoices? ChoicesFor(AbstractCreature abstractCreature) => abstractCreature.Game is { } game
        ? _choices.GetValue(game, game => new(game.StorySaveState))
        : null;

    extension(AbstractCreature abstractCreature)
    {
        internal bool IsExcluded => ChoicesFor(abstractCreature)?.Exclusions.Contains(abstractCreature.SaveDataKey) == true;

        internal bool HasFriendTracker => abstractCreature.IsSlugpup || abstractCreature.creatureTemplate is { IsLizard: true } or { IsCicada: true };

        internal void SetTracked(bool tracked)
        {
            if (abstractCreature.IsPlayer || ChoicesFor(abstractCreature) is not { } choices)
            {
                return;
            }

            if (choices.Choose(abstractCreature.SaveDataKey, tracked))
            {
                choices.Write();
            }
        }

        internal void Forget()
        {
            if (abstractCreature.IsPlayer)
            {
                return;
            }

            abstractCreature.state?.socialMemory?.relationShips?.RemoveAll(relationship => relationship.subjectID.IsKnownSlugcat);

            if (abstractCreature.abstractAI?.RealAI?.friendTracker is { } tracker && tracker.friend?.abstractCreature is { IsSlugcat: true })
            {
                tracker.friend = null;
                tracker.friendRel = null;
            }

            string key = abstractCreature.SaveDataKey;

            if (ChoicesFor(abstractCreature) is { } choices && (choices.Exclusions.Remove(key) | choices.Inclusions.Remove(key)))
            {
                choices.Write();
            }

            if (abstractCreature.Game is { } game && _metFriends.TryGetValue(game, out HashSet<AbstractCreature> metFriends))
            {
                metFriends.Remove(abstractCreature);
            }

            abstractCreature.SetNickname(null);
            abstractCreature.ForgetMeeting();
            abstractCreature.Untrack();
        }

        private void ApplyDefaultTracking()
        {
            if (ChoicesFor(abstractCreature) is not { } choices)
            {
                return;
            }

            string key = abstractCreature.SaveDataKey;

            if (!choices.Exclusions.Contains(key) && !choices.Inclusions.Contains(key) && choices.Choose(key, Config.TrackNewFriends.Value))
            {
                choices.Write();
            }
        }
    }

    private sealed class TrackingChoices
    {
        private const string SaveKey = "ABSOLUTEFRIENDS_TRACKING<svB>";

        private const string ExcludedSuffix = "=0";

        private const string IncludedSuffix = "=1";

        private const string PlayerPrefix = "P";

        internal readonly HashSet<string> Exclusions = new(StringComparer.Ordinal);

        internal readonly HashSet<string> Inclusions = new(StringComparer.Ordinal);

        private readonly SaveState? _save;

        internal TrackingChoices(SaveState? save)
        {
            _save = save;

            foreach (string entry in FriendSaveData.ReadEntries(save, SaveKey))
            {
                if (entry.StartsWith(PlayerPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (entry.EndsWith(ExcludedSuffix, StringComparison.Ordinal))
                {
                    Exclusions.Add(entry.Substring(0, entry.Length - ExcludedSuffix.Length));
                }
                else if (entry.EndsWith(IncludedSuffix, StringComparison.Ordinal))
                {
                    Inclusions.Add(entry.Substring(0, entry.Length - IncludedSuffix.Length));
                }
            }
        }

        internal bool Choose(string key, bool tracked) => tracked
            ? Inclusions.Add(key) | Exclusions.Remove(key)
            : Exclusions.Add(key) | Inclusions.Remove(key);

        internal void Write() => FriendSaveData.WriteEntries(_save, SaveKey, Exclusions
            .Select(key => key + ExcludedSuffix)
            .Concat(Inclusions.Select(key => key + IncludedSuffix))
            .OrderBy(entry => entry, StringComparer.Ordinal));
    }
}
