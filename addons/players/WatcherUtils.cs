using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;
using Watcher;

namespace AbsoluteFriends.Players;

internal static class WatcherUtils
{
    extension(Player? player)
    {
        public bool IsWatcher => player?.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher;

        public bool IsCamouflagePlayer => player is { dead: false } && player.IsPlayer && player.IsWatcher;

        public bool IsTrackedWatcher => player.IsCamouflagePlayer && player.IsTracked;
    }
}
