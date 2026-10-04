using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Visualizer;

internal static class OutsideFriends
{
    private static readonly HashSet<AbstractCreature> _outside = [];

    private static RainWorldGame? _game;

    private static int _clock = -1;

    private static void Refresh(RainWorldGame? game)
    {
        if (game == null || (ReferenceEquals(game, _game) && game.clock == _clock))
        {
            return;
        }

        _game = game;
        _clock = game.clock;
        _outside.Clear();

        if (WaitingRoom(game) is not { } room)
        {
            return;
        }

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
    }
}
