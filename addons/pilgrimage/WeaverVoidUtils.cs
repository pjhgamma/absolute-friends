using AbsoluteFriends.Core;

namespace AbsoluteFriends.Pilgrimage;

internal static class WeaverVoidUtils
{
    private const string WeaverRoom = "WRSA_WEAVER";

    public static bool IsWeaverRoom(Room? room)
    {
        return room is { game.session: StoryGameSession { finalWarpSequenceStarted: true } }
            && string.Equals(room.abstractRoom.name, WeaverRoom, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsInWeaverVoid(Creature creature)
    {
        return creature is not Player && creature.abstractCreature.IsCompanion && IsWeaverRoom(creature.room);
    }
}
