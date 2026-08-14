namespace RippleFriends.Core;

internal static class GameUtils
{
    public static RoomCamera? MainCamera(RainWorldGame? game) => game?.cameras is { Length: > 0 } cameras ? cameras[0] : null;
}
