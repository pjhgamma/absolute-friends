using AbsoluteFriends.Core;
using UnityEngine;
using VoidSea;

namespace AbsoluteFriends.Pilgrimage;

internal static partial class VoidSeaUtils
{
    private const float RowSpacing = 40f;

    private const int ReleaseTime = 400;

    public static void FollowWormRide(VoidSeaScene scene, VoidSeaState state, Creature[] companions, HashSet<Creature> riders)
    {
        if (!scene.IsWormRiding || Lead(scene.room.game) is not { } lead || lead.room != scene.room)
        {
            return;
        }

        Vector2 leadPos = lead.mainBodyChunk.pos;
        Vector2 velocity = lead.mainBodyChunk.vel;
        int slot = 0;
        HashSet<PhysicalObject> moved = [];

        foreach (var player in OtherTrackedPlayers(scene, lead))
        {
            Vector2 delta = leadPos + new Vector2(RowOffset(slot++), 0f) - player.mainBodyChunk.pos;

            Carry(player, delta, velocity);

            foreach (var grasp in player.grasps)
            {
                if (grasp?.grabbed is { } held && held.room == scene.room && moved.Add(held))
                {
                    Carry(held, delta, velocity);

                    if (held is Creature creature)
                    {
                        riders.Add(creature);
                    }
                }
            }

            foreach (var companion in companions)
            {
                if (companion is Player { onBack: var carrier } && carrier == player && moved.Add(companion))
                {
                    Carry(companion, delta, velocity);

                    riders.Add(companion);
                }
            }
        }

        foreach (var companion in companions)
        {
            if (!companion.IsCarried && moved.Add(companion))
            {
                Carry(companion, leadPos + new Vector2(RowOffset(slot++), 0f) - companion.mainBodyChunk.pos, velocity);
            }

            riders.Add(companion);
        }

        if (scene.MainWormBehavior is { } behavior && behavior.phase == VoidWorm.MainWormBehavior.Phase.DepthReached && behavior.timeInPhase >= ReleaseTime)
        {
            state.RideReleased = true;
        }
    }

    public static void AttachThreads(VoidSeaScene scene, VoidSeaState state, Creature[] companions)
    {
        VoidWorm? worm = scene.worms.FirstOrDefault(worm => worm.mainWorm && worm.arms.Any(HasThread));

        if (worm?.arms.FirstOrDefault(HasThread) is not { } arm)
        {
            return;
        }

        IEnumerable<Creature> riders = Lead(scene.room.game) is { } lead && lead.room == scene.room
            ? companions.Concat(OtherTrackedPlayers(scene, lead))
            : companions;

        foreach (var rider in riders)
        {
            if (state.Threads.TryGetValue(rider, out WormThreadOverlay existing) && !existing.slatedForDeletetion)
            {
                continue;
            }

            WormThreadOverlay overlay = new(scene, worm, arm, rider);

            state.Threads[rider] = overlay;
            scene.room.AddObject(overlay);
        }
    }

    private static float RowOffset(int slot)
    {
        int step = slot / 2 + 1;

        return slot % 2 == 0 ? -step * RowSpacing : step * RowSpacing;
    }

    private static bool HasThread(VoidWorm.Arm arm)
    {
        return arm.thread != null && arm.threadStatus > 0;
    }

    private static IEnumerable<Player> OtherTrackedPlayers(VoidSeaScene scene, Player lead)
    {
        foreach (var abstractPlayer in scene.room.game.Players)
        {
            if (abstractPlayer.IsTracked && abstractPlayer.realizedCreature is Player player && player != lead && player.room == scene.room)
            {
                yield return player;
            }
        }
    }

    private static void Carry(PhysicalObject physicalObject, Vector2 delta, Vector2 velocity)
    {
        if (delta.sqrMagnitude >= 1f)
        {
            Shift(physicalObject, delta);
        }

        SetVelocity(physicalObject, velocity);
    }
}
