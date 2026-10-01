using System.Text.RegularExpressions;
using AbsoluteFriends.Core;

namespace AbsoluteFriends.Pilgrimage;

internal static class SaveStateUtils
{
    private static string SavedId(string entry)
    {
        string[] fields = Regex.Split(entry, "<cA>");

        return fields.Length > 1 ? Regex.Split(fields[1], "<cB>")[0] : string.Empty;
    }

    extension(SaveState saveState)
    {
        public void KeepEndingCompanions(RainWorldGame game)
        {
            List<string> pending = saveState.pendingFriendCreatures;

            pending.AddRange(saveState.spawnedPendingFriendCreatures.Except(pending));

            HashSet<AbstractRoom> playerRooms = [.. game.Players
                .Where(abstractPlayer => abstractPlayer.realizedCreature is { dead: false })
                .Select(abstractPlayer => abstractPlayer.Room)];
            AbstractCreature[] companions = [.. FriendUtils.TrackedFriends
                .Where(abstractFriend => abstractFriend is { IsCompanion: true, state.alive: true } && abstractFriend.world == game.world && playerRooms.Contains(abstractFriend.Room))];

            if (companions.Length == 0)
            {
                return;
            }

            HashSet<string> ids = [.. companions.Select(abstractFriend => abstractFriend.ID.ToString())];

            pending.RemoveAll(entry => ids.Contains(SavedId(entry)));
            game.world?.regionState?.savedPopulation.RemoveAll(entry => ids.Contains(SavedId(entry)));
            pending.AddRange(companions.Select(SaveState.AbstractCreatureToStringStoryWorld));
        }
    }
}
