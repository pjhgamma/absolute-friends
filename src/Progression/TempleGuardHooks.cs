using RippleFriends.Friends;
using RippleFriends.Hooks;
using RippleFriends.Options;
using RippleFriends.Utils;
using UnityEngine;

namespace RippleFriends.Progression;

internal class TempleGuardHooks : BaseHooks
{
    protected override Configurable<bool>[] Options => [Config.TempleGuard];

    private static bool IsPassedFriend(PhysicalObject? physicalObject)
    {
        RainWorldGame? game = physicalObject?.room?.game;

        foreach (var player in game.RealizedPlayers)
        {
            if (
                player.KarmaCap + (game?.bestHeldScavenger != null ? game.karmaOfBestHeldScavenger : 0) >= 9
                && physicalObject.IsFriend(player)
            )
            {
                return true;
            }
        }

        return false;
    }

    [HookPatch(typeof(On.TempleGuardAI), nameof(On.TempleGuardAI.ThrowOutScore))]
    private static float On_TempleGuardAI_ThrowOutScore(On.TempleGuardAI.orig_ThrowOutScore orig, TempleGuardAI self, global::Tracker.CreatureRepresentation crit)
    {
        return IsPassedFriend(crit?.representedCreature?.realizedCreature) ? 0f : orig(self, crit);
    }

    [HookPatch(typeof(On.TempleGuard), nameof(On.TempleGuard.Act))]
    private static void On_TempleGuard_Act(On.TempleGuard.orig_Act orig, TempleGuard self, bool eu)
    {
        List<(BodyChunk bodyChunk, Vector2 pos, Vector2 vel)> bodyChunks = [];

        foreach (var creature in self.room.FriendsOfPlayer)
        {
            if (IsPassedFriend(creature))
            {
                foreach (var bodyChunk in creature.bodyChunks ?? [])
                {
                    if (bodyChunk != null)
                    {
                        bodyChunks.Add((bodyChunk, bodyChunk.pos, bodyChunk.vel));
                    }
                }
            }
        }

        orig(self, eu);

        foreach (var (bodyChunk, pos, vel) in bodyChunks)
        {
            bodyChunk.pos = pos;
            bodyChunk.vel = vel;
        }
    }
}
