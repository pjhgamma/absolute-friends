using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using RWCustom;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AbsoluteFriends.Progression;

internal enum ResonanceType
{
    Automatic,
    Gathered,
    Ungathered
}

internal readonly struct ResonanceProfile(float damage, float permanent, float poison, bool dead)
{
    public readonly float Damage = damage;

    public readonly float PermanentDamage = permanent;

    public readonly float Poison = poison;

    public readonly bool Dead = dead;

    public static ResonanceProfile Zero => new(0f, 0f, 0f, false);

    public override string ToString()
    {
        return $"(damage {Damage:0.00}, permanent {PermanentDamage:0.00}, poison {Poison:0.00}, dead {Dead})";
    }

    public ResonanceProfile Min(ResonanceProfile other)
    {
        return new(
            Mathf.Min(Damage, other.Damage),
            Mathf.Min(PermanentDamage, other.PermanentDamage),
            Mathf.Min(Poison, other.Poison),
            Dead && other.Dead
        );
    }
}

internal static class ResonanceUtils
{
    private static readonly Func<Creature, bool> _isHeld = _ => true;

    private static ConditionalWeakTable<AbstractCreature, ResonanceTracker> _trackers = new();

    private static float CostRatio => Config.ResonanceCost.IsActive ? Config.ResonanceCostRatio.Value : 0f;

    private static float AftershockRatio => Config.ResonanceAftershock.IsActive ? Config.ResonanceAftershockRatio.Value : 0f;

    public static void ResetTrackers()
    {
        _trackers = new();
    }

    extension(AbstractCreature abstractCreature)
    {
        public ResonanceTracker Tracker => _trackers.GetOrCreateValue(abstractCreature);

        public ResonanceTracker? FindTracker()
        {
            return _trackers.TryGetValue(abstractCreature, out ResonanceTracker tracker) ? tracker : null;
        }

        private bool IsInRoom(Room room)
        {
            return abstractCreature.world == room.world && abstractCreature.Room == room.abstractRoom;
        }

        private bool AwaitWarp(Func<Creature, bool> predicate)
        {
            return abstractCreature.realizedCreature is not { } creature || !predicate(creature);
        }

        private bool AwaitMend(ResonanceProfile profile)
        {
            return abstractCreature.GetMendCost(profile) > 0;
        }

        private bool CanWarp(Func<Creature, bool> predicate, ResonanceProfile profile)
        {
            return Config.ResonanceWarp.IsActive && abstractCreature.AwaitWarp(predicate) && (!abstractCreature.AwaitMend(profile) || Config.ResonanceMend.IsActive);
        }

        private bool CanMend(Func<Creature, bool> predicate, ResonanceProfile profile)
        {
            return Config.ResonanceMend.IsActive && abstractCreature.AwaitMend(profile) && (!abstractCreature.AwaitWarp(predicate) || Config.ResonanceWarp.IsActive);
        }

        private bool CanWarpOrMend(Func<Creature, bool> predicate, ResonanceProfile profile)
        {
            return abstractCreature.CanWarp(predicate, profile) || abstractCreature.CanMend(predicate, profile);
        }

        private void Release(RainWorldGame? game)
        {
            if (abstractCreature.realizedCreature is { } creature)
            {
                creature.LoseAllGrasps();
                creature.AllGraspsLetGoOfThisObject(true);
                creature.ReleaseShortcuts(game);
                creature.CollideWithTerrain = true;
            }

            abstractCreature.LoseAllStuckObjects();
            abstractCreature.InDen = false;
            abstractCreature.remainInDenCounter = 0;
        }

        private void Warp(World world, WorldCoordinate worldCoordinate)
        {
            AbstractRoom? abstractRoom = world?.GetAbstractRoom(worldCoordinate);

            if (abstractRoom?.realizedRoom is not Room room)
            {
                return;
            }

            World previousWorld = abstractCreature.world;
            AbstractRoom previousAbstractRoom = abstractCreature.Room;

            if (abstractCreature.realizedCreature is { slatedForDeletetion: true })
            {
                abstractCreature.realizedCreature = null;
            }

            abstractCreature.Release(room.game);

            Creature? creature = abstractCreature.realizedCreature;

            if (creature?.room is { } previousRoom && previousRoom != room)
            {
                previousRoom.RemoveObject(creature);
            }

            previousAbstractRoom?.RemoveEntity(abstractCreature);

            abstractCreature.slatedForDeletion = false;
            abstractCreature.world = world;
            abstractCreature.pos = worldCoordinate;
            abstractCreature.timeSpentHere = 0;

            abstractRoom.AddEntity(abstractCreature);

            if (previousWorld != world && abstractCreature.creatureTemplate?.AI == true)
            {
                abstractCreature.abstractAI?.NewWorld(world);
                abstractCreature.InitiateAI();
            }

            if (creature == null)
            {
                abstractCreature.RealizeInRoom();
            }
            else
            {
                IntVector2 tile = abstractCreature.pos.Tile;

                creature.ClearShortcut();

                if (creature.room != room)
                {
                    creature.SpitOutOfShortCut(tile, room, true);

                    return;
                }

                Vector2 position = room.MiddleOfTile(tile);

                foreach (var bodyChunk in creature.bodyChunks ?? [])
                {
                    bodyChunk.HardSetPosition(position + Custom.RNV());
                    bodyChunk.vel *= 0f;
                }
            }
        }

        private void Mend(ResonanceProfile profile)
        {
            bool shouldRevive = !profile.Dead;

            abstractCreature.slatedForDeletion = false;

            if (abstractCreature.state is CreatureState creatureState)
            {
                if (shouldRevive)
                {
                    creatureState.alive = true;
                }
                if (creatureState is PlayerState playerState)
                {
                    if (shouldRevive)
                    {
                        playerState.permaDead = false;
                    }
                    playerState.permanentDamageTracking = Mathf.Min((float)playerState.permanentDamageTracking, profile.PermanentDamage);
                }
                if (creatureState is HealthState healthState)
                {
                    healthState.health = Mathf.Max(healthState.health, 1f - profile.Damage);
                }
            }

            if (abstractCreature.realizedCreature is { } creature)
            {
                if (shouldRevive)
                {
                    creature.dead = false;
                    creature.killTag = null;
                    creature.killTagCounter = 0;
                }
                creature.injectedPoison = Mathf.Min(creature.injectedPoison, profile.Poison);
            }
        }

        private void WarpAndMend(World world, WorldCoordinate worldCoordinate, Func<Creature, bool> predicate, ResonanceProfile profile)
        {
            bool canWarp = abstractCreature.CanWarp(predicate, profile);
            bool canMend = abstractCreature.CanMend(predicate, profile);

            if (canMend)
            {
                abstractCreature.Mend(profile);
            }
            if (canWarp)
            {
                abstractCreature.Warp(world, worldCoordinate);
            }
        }

        private ResonanceProfile Profile
        {
            get
            {
                Creature? creature = abstractCreature.realizedCreature;

                return new(
                    abstractCreature.state is HealthState healthState ? Mathf.Max(1f - healthState.health, 0f) : 0f,
                    abstractCreature.state is PlayerState playerState ? Mathf.Max((float)playerState.permanentDamageTracking, 0f) : 0f,
                    Mathf.Max(creature?.injectedPoison ?? 0f, 0f),
                    abstractCreature.state?.dead == true
                );
            }
        }

        private int GetWarpCost(Room room)
        {
            int cost = 0;

            if (abstractCreature.world == null || abstractCreature.world != room.world)
            {
                cost += 10 * 60 * RainWorldUtils.Second;
            }
            else if (abstractCreature.Room?.realizedRoom != room)
            {
                cost += 60 * RainWorldUtils.Second;
            }
            else
            {
                cost += 10 * RainWorldUtils.Second;
            }

            if (abstractCreature.realizedCreature is not { } creature || abstractCreature.slatedForDeletion)
            {
                cost += 60 * RainWorldUtils.Second;
            }
            else if (creature.inShortcut || creature.enteringShortCut != null)
            {
                cost += 10 * RainWorldUtils.Second;
            }

            if (abstractCreature.InDen)
            {
                cost += 60 * RainWorldUtils.Second;
            }
            if (abstractCreature.stuckObjects?.Count > 0)
            {
                cost += abstractCreature.stuckObjects.Count * 10 * RainWorldUtils.Second;
            }

            return cost;
        }

        private int GetMendCost(ResonanceProfile profile)
        {
            ResonanceProfile abstractCreatureProfile = abstractCreature.Profile;
            float damage = 0f;
            int cost = 0;

            damage += Mathf.Max(abstractCreatureProfile.Damage - profile.Damage, 0f);
            damage += Mathf.Max(abstractCreatureProfile.PermanentDamage - profile.PermanentDamage, 0f);
            damage += Mathf.Max(abstractCreatureProfile.Poison - profile.Poison, 0f);
            cost += (int)(damage * (abstractCreature.creatureTemplate?.baseDamageResistance ?? 1f) * 60 * RainWorldUtils.Second);
            if (abstractCreatureProfile.Dead && !profile.Dead)
            {
                cost += Mathf.Max(abstractCreature.Tracker.Death, 1);
            }

            return cost;
        }

        private int GetCost(Room room, Func<Creature, bool> predicate, ResonanceProfile profile)
        {
            int cost = 0;

            if (abstractCreature.CanWarp(predicate, profile))
            {
                cost += abstractCreature.GetWarpCost(room);
            }

            if (abstractCreature.CanMend(predicate, profile))
            {
                cost += abstractCreature.GetMendCost(profile);
            }

            return (int)Mathf.Sqrt(cost * CostRatio);
        }

        private void Reflect(int duration, int cost)
        {
            if (duration < 1)
            {
                return;
            }

            ResonanceTracker tracker = abstractCreature.Tracker;

            tracker.Duration = duration;
            tracker.Reflection = duration;
            tracker.Aftershock += (int)(cost * AftershockRatio);

            if (abstractCreature.realizedCreature is not { } creature)
            {
                return;
            }

            creature.Stun(duration);

            if (creature is Player player)
            {
                player.Blink(duration);
                player.airInLungs *= 0.1f;
                player.exhausted = true;
                player.aerobicLevel = Mathf.Max(player.aerobicLevel, 1.5f);
            }
        }
    }

    extension(IEnumerable<AbstractCreature> abstractCreatures)
    {
        private ResonanceProfile Profile => abstractCreatures.Select(abstractCreature => abstractCreature.Profile).DefaultIfEmpty(ResonanceProfile.Zero).Aggregate((profile, other) => profile.Min(other));
    }

    extension(int cost)
    {
        public float GetProgress(int reflection)
        {
            return cost == 0 ? 1f : Mathf.InverseLerp(0, cost, reflection);
        }
    }

    extension(Creature creature)
    {
        public void Vibrate(float progress)
        {
            Vector2 vector = Custom.RNV();

            foreach (var bodyChunk in creature.bodyChunks ?? [])
            {
                vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
                vector *= Mathf.Min(3f, Random.value * 3f / Mathf.Lerp(bodyChunk.mass, 1f, 0.5f)) * progress;
                bodyChunk.pos += vector;
                bodyChunk.vel += vector * 0.5f;
            }

            foreach (var bodyPart in creature.graphicsModule?.bodyParts ?? [])
            {
                vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
                vector *= Random.value * 2f * progress;
                bodyPart.pos += vector;
                bodyPart.vel += vector;

                if (bodyPart is Limb limb)
                {
                    limb.mode = Limb.Mode.Dangle;
                }
            }

            if (creature is Player player)
            {
                player.Blink(5);
            }
        }

        public void Stagger(float severity)
        {
            if (severity <= 0f || !creature.Consious || Random.value >= severity)
            {
                return;
            }

            creature.Stun(Random.Range(20, 80));

            if (creature is Player player)
            {
                player.Blink(100);
                player.exhausted = true;
            }
        }

        private void ClearShortcut()
        {
            creature.inShortcut = false;
            creature.inShortcutVessel = null;
            creature.enteringShortCut = null;
        }

        private void ReleaseShortcuts(RainWorldGame? game)
        {
            bool shortcutRemoved = false;

            if (game?.shortcuts is { } shortcutHandler)
            {
                shortcutRemoved |= shortcutHandler.transportVessels?.RemoveAll(vessel => vessel?.creature == creature) > 0;
                shortcutRemoved |= shortcutHandler.betweenRoomsWaitingLobby?.RemoveAll(vessel => vessel?.creature == creature) > 0;
                shortcutRemoved |= shortcutHandler.borderTravelVessels?.RemoveAll(vessel => vessel?.creature == creature) > 0;
            }

            creature.ClearShortcut();

            if (!shortcutRemoved)
            {
                return;
            }

            foreach (var abstractPhysicalObject in creature.abstractCreature?.GetAllConnectedObjects() ?? [])
            {
                if (abstractPhysicalObject?.realizedObject is Creature connectedCreature && connectedCreature != creature)
                {
                    connectedCreature.ClearShortcut();
                }
            }
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

            if (!isUngathered && abstractPlayers.Any(abstractPlayer => abstractPlayer?.realizedCreature is not Player player || !predicate(player)))
            {
                return [];
            }

            return [.. abstractPlayers];
        }

        private IEnumerable<AbstractCreature> Candidates(Func<Creature, bool> predicate, ResonanceType type)
        {
            foreach (var abstractCreature in FriendUtils.TrackedFriendsIncludingPlayers)
            {
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
            List<AbstractCreature> candidates = [.. room.Candidates(predicate, type)];
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
            ResonanceType type = room.IsGathered(resonators) ? ResonanceType.Gathered : ResonanceType.Ungathered;
            List<AbstractCreature> resonatees = room.Resonatees(resonators, predicate, type, out ResonanceProfile profile);

            if (resonators.Count == 0 || resonatees.Count == 0)
            {
                resonanceAnchor.Players.Clear();
                resonanceAnchor.Reflection = 0;

                return false;
            }

            HashSet<Player> players = room.game?.AlivePlayers.Select(abstractPlayer => abstractPlayer.realizedCreature).OfType<Player>().ToHashSet() ?? [];

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
                        || !abstractCreature.IsTracked
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

    private static bool ShouldVibrate(float progress, int reflection) => progress >= 0.1f || reflection >= 5;
}
