using System.Runtime.CompilerServices;
using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Resonance;

internal enum ResonanceType
{
    Automatic,
    Gathered,
    Ungathered
}

internal static partial class ResonanceUtils
{
    private static readonly Func<Creature, bool> _isHeld = _ => true;

    private static ConditionalWeakTable<AbstractCreature, ResonanceTracker> _trackers = new();

    private static float CostRatio => Config.ResonanceCost.IsActive ? Config.ResonanceCostRatio.Value : 0f;

    public static void ResetTrackers()
    {
        _trackers = new();
    }

    private static bool ShouldVibrate(float progress, int reflection) => progress >= 0.1f || reflection >= 5;

    extension(int cost)
    {
        public float GetProgress(int reflection)
        {
            return cost == 0 ? 1f : Mathf.InverseLerp(0, cost, reflection);
        }
    }

    extension(Player player)
    {
        public bool IsResonating
        {
            get
            {
                Player.InputPackage input = player.input[0];

                return input.x == 0 && input.y == 0 && input.jmp && !input.thrw && !input.pckp && !input.spec;
            }
        }
    }

    extension(Room room)
    {
        private bool IsGathered(List<AbstractCreature> resonators)
        {
            return Config.ResonanceGate.IsActive
                && resonators.Count > 0
                && resonators.All(abstractCreature => abstractCreature.realizedCreature is { } creature && room.IsInGate(creature));
        }

        private List<AbstractCreature> Resonators(Func<Creature, bool> predicate, bool isUngathered = false)
        {
            List<AbstractCreature> abstractPlayers = room.game?.AlivePlayers ?? [];

            if (!Core.Config.FriendSharing.IsActive)
            {
                return [.. abstractPlayers.Where(abstractPlayer => abstractPlayer?.realizedCreature is Player player && predicate(player))];
            }

            if (!isUngathered && abstractPlayers.Any(abstractPlayer => abstractPlayer?.realizedCreature is not Player player || !predicate(player)))
            {
                return [];
            }

            return [.. abstractPlayers];
        }

        private IEnumerable<AbstractCreature> Candidates(List<AbstractCreature> resonators, Func<Creature, bool> predicate, ResonanceType type)
        {
            foreach (var abstractCreature in FriendUtils.TrackedFriendsIncludingPlayers)
            {
                if (!Core.Config.FriendSharing.IsActive && !resonators.Any(abstractPlayer => abstractCreature.IsTrackedFor(abstractPlayer)))
                {
                    continue;
                }

                if (type == ResonanceType.Ungathered && !abstractCreature.IsInRoom(room))
                {
                    continue;
                }

                if (abstractCreature.CanWarpOrMend(predicate, ResonanceProfile.Zero))
                {
                    yield return abstractCreature;
                }
            }
        }

        private List<AbstractCreature> Resonatees(List<AbstractCreature> resonators, Func<Creature, bool> predicate, ResonanceType type, out ResonanceProfile profile)
        {
            List<AbstractCreature> candidates = [.. room.Candidates(resonators, predicate, type)];
            ResonanceProfile baseProfile = candidates.Union(resonators).Profile;

            profile = baseProfile;

            return [.. candidates.Where(abstractCreature => abstractCreature.CanWarpOrMend(predicate, baseProfile))];
        }

        private int GetCost(IEnumerable<AbstractCreature> resonatees, Func<Creature, bool> predicate, ResonanceProfile profile)
        {
            return resonatees.Sum(abstractCreature => abstractCreature.GetCost(room, predicate, profile));
        }

        private void Echo(ResonanceType type, List<AbstractCreature> resonated)
        {
            if (!Config.ResonanceEffect.IsActive)
            {
                return;
            }

            if (type == ResonanceType.Automatic)
            {
                room.AddObject(new ResonanceEffect(room));
            }
            room.AddObject(new ResonanceBurst(room, resonated));
        }

        private bool Resonate(List<AbstractCreature> resonators, List<AbstractCreature> resonatees, ResonanceProfile profile, Func<Creature, bool> predicate, ResonanceType type)
        {
            if (
                resonators.Count == 0
                || resonatees.Count == 0
                || resonators.FirstOrDefault(abstractCreature => !abstractCreature.AwaitWarp(predicate)) is not { } origin
            )
            {
                return false;
            }

            List<AbstractCreature> resonatedCreatures = [.. resonatees.Union(resonators)];
            World world = origin.world;
            WorldCoordinate worldCoordinate = origin.pos;
            int cost = room.GetCost(resonatees, predicate, profile);
            int duration = cost / resonators.Count;
            int sharedCost = cost / resonatedCreatures.Count;

            foreach (var abstractCreature in resonatees)
            {
                abstractCreature.WarpAndMend(world, worldCoordinate, predicate, profile);
                abstractCreature.Reflect(duration, sharedCost);
            }

            if (type != ResonanceType.Automatic)
            {
                foreach (var abstractPlayer in resonators.Except(resonatees))
                {
                    abstractPlayer.Reflect(duration, sharedCost);
                }
            }

            room.Echo(type, resonatedCreatures);

            return true;
        }

        private bool Resonate(Func<Creature, bool> predicate, ResonanceType type, bool isUngathered = false)
        {
            List<AbstractCreature> resonators = room.Resonators(predicate, isUngathered);

            return room.Resonate(resonators, room.Resonatees(resonators, predicate, type, out ResonanceProfile profile), profile, predicate, type);
        }
    }

    extension<T>(T gate) where T : UpdatableAndDeletable
    {
        public void Resonate(Func<T, Creature, bool> isInGate)
        {
            gate.room?.Resonate(creature => isInGate(gate, creature), ResonanceType.Automatic);
        }

        public void ResonateApart(Func<T, Creature, bool> isInGate)
        {
            gate.room?.Resonate(creature => isInGate(gate, creature), ResonanceType.Automatic, isUngathered: true);
        }
    }

    extension(ResonanceAnchor resonanceAnchor)
    {
        public bool ResonateRoom()
        {
            Room room = resonanceAnchor.room;
            Func<Creature, bool> predicate = resonanceAnchor.Predicate;
            List<AbstractCreature> resonators = room.Resonators(predicate);

            if (!Core.Config.FriendSharing.IsActive)
            {
                resonators.RemoveAll(abstractPlayer => abstractPlayer.realizedCreature is not Player player || !player.IsResonating);
            }

            ResonanceType type = room.IsGathered(resonators) ? ResonanceType.Gathered : ResonanceType.Ungathered;
            List<AbstractCreature> resonatees = room.Resonatees(resonators, predicate, type, out ResonanceProfile profile);

            if (resonators.Count == 0 || resonatees.Count == 0)
            {
                resonanceAnchor.Players.Clear();
                resonanceAnchor.Reflection = 0;

                return false;
            }

            HashSet<Player> players = [.. resonators.Select(abstractPlayer => abstractPlayer.realizedCreature).OfType<Player>()];

            if (!resonanceAnchor.Players.SetEquals(players) || !players.All(player => player.IsResonating))
            {
                resonanceAnchor.Players = players;
                resonanceAnchor.Reflection = 0;

                return false;
            }

            int cost = room.GetCost(resonatees, predicate, profile);
            float progress = cost.GetProgress(++resonanceAnchor.Reflection * players.Count);

            if (progress >= 1f)
            {
                resonanceAnchor.Reflection = 0;

                return room.Resonate(resonators, resonatees, profile, predicate, type);
            }

            if (ShouldVibrate(progress, resonanceAnchor.Reflection))
            {
                foreach (var player in players)
                {
                    player.Vibrate(progress);
                }
            }

            return false;
        }

        public bool ResonateGrab()
        {
            Room room = resonanceAnchor.room;
            Dictionary<AbstractCreature, int> grasps = resonanceAnchor.Grasps;
            HashSet<AbstractCreature> held = [];

            foreach (var abstractPlayer in room.game?.AlivePlayers ?? [])
            {
                if (abstractPlayer?.realizedCreature is not Player player || player.room != room || !player.IsResonating)
                {
                    continue;
                }

                foreach (var grabbed in player.HeldObjects)
                {
                    if (
                        grabbed.abstractPhysicalObject is not AbstractCreature abstractCreature
                        || !(Core.Config.FriendSharing.IsActive ? abstractCreature.IsTracked : abstractCreature.IsTrackedFor(abstractPlayer))
                        || !abstractCreature.IsInRoom(room)
                        || !held.Add(abstractCreature))
                    {
                        continue;
                    }

                    ResonanceProfile profile = abstractPlayer.Profile.Min(abstractCreature.Profile);

                    if (!abstractCreature.CanMend(_isHeld, profile))
                    {
                        continue;
                    }

                    int reflection = (grasps.TryGetValue(abstractCreature, out int value) ? value : 0) + 1;
                    float progress = abstractCreature.GetCost(room, _isHeld, profile).GetProgress(reflection);

                    grasps[abstractCreature] = reflection;

                    if (progress >= 1f)
                    {
                        grasps.Remove(abstractCreature);

                        if (room.Resonate([abstractPlayer], [abstractCreature], profile, _isHeld, ResonanceType.Ungathered))
                        {
                            return true;
                        }
                    }
                    else if (ShouldVibrate(progress, reflection))
                    {
                        player.Vibrate(progress);

                        abstractCreature.realizedCreature?.Vibrate(progress);
                    }
                }
            }

            return false;
        }
    }
}
