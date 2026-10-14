using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal static class OutsideFriends
{
    private static readonly HashSet<AbstractCreature> _outside = [];

    private static readonly Dictionary<AbstractCreature, float> _intensities = [];

    private static RainWorldGame? _game;

    private static int _clock = -1;

    internal static void ClearGame()
    {
        _outside.Clear();
        _intensities.Clear();
        _game = null;
        _clock = -1;
    }

    private static void Refresh(RainWorldGame? game)
    {
        if (game == null || (ReferenceEquals(game, _game) && game.clock == _clock))
        {
            return;
        }

        if (!ReferenceEquals(game, _game))
        {
            _intensities.Clear();
        }

        _game = game;
        _clock = game.clock;
        _outside.Clear();

        if (WaitingRoom(game) is { } room)
        {
            foreach (var abstractCreature in FriendUtils.TrackedFriends)
            {
                if (abstractCreature.IsPlayer || abstractCreature.slatedForDeletion || abstractCreature.IsDead)
                {
                    continue;
                }

                if (abstractCreature.realizedCreature is not { } creature || !room.IsInGate(creature))
                {
                    _outside.Add(abstractCreature);
                }
            }
        }

        foreach (var abstractCreature in _intensities.Keys.Union(_outside).ToList())
        {
            float intensity = Mathf.MoveTowards(_intensities.TryGetValue(abstractCreature, out float value) ? value : 0f, _outside.Contains(abstractCreature) ? 1f : 0f, OverlayUtils.GatePulseStep);

            if (intensity > 0f)
            {
                _intensities[abstractCreature] = intensity;
            }
            else
            {
                _intensities.Remove(abstractCreature);
            }
        }
    }

    private static Room? WaitingRoom(RainWorldGame game)
    {
        Room? room = null;

        foreach (var abstractPlayer in game.AlivePlayers)
        {
            if (abstractPlayer.realizedCreature is not Player { IsPlayer: true, room: { } playerRoom } player)
            {
                continue;
            }

            if ((room != null && room != playerRoom) || !playerRoom.IsInGate(player))
            {
                return null;
            }

            room = playerRoom;
        }

        return room;
    }

    extension(AbstractPhysicalObject source)
    {
        public bool IsOutsideGate
        {
            get
            {
                Refresh(source.world?.game);

                return source is AbstractCreature abstractCreature && _outside.Contains(abstractCreature);
            }
        }

        public float GatePulseIntensity
        {
            get
            {
                Refresh(source.world?.game);

                return source is AbstractCreature abstractCreature && _intensities.TryGetValue(abstractCreature, out float intensity) ? intensity : 0f;
            }
        }
    }
}
