using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;
using Watcher;

namespace AbsoluteFriends.Pilgrimage;

internal static class WarpPointUtils
{
    extension(WarpPoint warpPoint)
    {
        public bool IsVoidWarp => warpPoint.canWarpToVoidWeaverEnding
            || (warpPoint.guaranteeTrigger && warpPoint.room?.game?.session is StoryGameSession { finalWarpSequenceStarted: true });

        public void BringCompanions()
        {
            foreach (var abstractFriend in FriendUtils.TrackedFriends.ToArray())
            {
                if (
                    abstractFriend is { IsCompanion: true, realizedCreature: { dead: false, inShortcut: false, Carrier: null } friend }
                    && friend.room == warpPoint.room
                    && !warpPoint.IsInWarpPoint(friend)
                )
                {
                    VoidSeaUtils.Shift(friend, warpPoint.pos - friend.mainBodyChunk.pos);
                    VoidSeaUtils.Stop(friend);
                }
            }
        }
    }
}
