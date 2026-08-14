using RippleFriends.Core;
using RippleFriends.Diagnostics;
using RWCustom;
using UnityEngine;

namespace RippleFriends.Objects;

internal static class LinkOverlayUtils
{
    public const float ViewMargin = 20f;

    public static bool IsOverlay(this UpdatableAndDeletable? obj) => obj is OwnerOverlay or FriendLinkOverlay;

    public static bool ShouldDraw(RoomCamera? camera, Room? room) => camera != null && camera.room == room && camera == GameUtils.MainCamera(room?.game);

    public static bool IsOnScreen(this Vector2 position, RoomCamera camera) => camera.PositionCurrentlyVisible(position, ViewMargin, false);

    public static bool TryGetPosition(this UpdatableAndDeletable? source, float timeStacker, out Vector2 position)
    {
        Vector2? found = source switch
        {
            Creature { mainBodyChunk: { } main } => Interpolate(main, timeStacker),
            PhysicalObject { firstChunk: { } first } => Interpolate(first, timeStacker),
            Explosion explosion => explosion.pos,
            SporePlant.Bee bee => Interpolate(bee.lastPos, bee.pos, timeStacker),
            _ => null,
        };

        position = found ?? default;

        return found.HasValue;
    }

    public static bool TryGetLine(RoomCamera camera, UpdatableAndDeletable? start, UpdatableAndDeletable? end, float timeStacker, out Vector2 first, out Vector2 second)
    {
        if (
            start.TryGetPosition(timeStacker, out first)
            && end.TryGetPosition(timeStacker, out second)
            && first.IsOnScreen(camera)
            && second.IsOnScreen(camera)
        )
        {
            return true;
        }

        first = second = default;

        return false;
    }

    public static void ApplyHologram(this FFacetNode node)
    {
        try
        {
            if (Custom.rainWorld?.Shaders is { } shaders && shaders.TryGetValue("Hologram", out FShader shader))
            {
                node.shader = shader;
            }
        }
        catch (Exception exception)
        {
            HookDiagnostics.LogWarning($"Could not apply the hologram shader", exception);
        }
    }

    public static void Destroy(this UpdatableAndDeletable overlay, Exception exception)
    {
        overlay.Destroy();

        HookDiagnostics.LogError($"{overlay.GetType().Name} failed and was removed", exception);
    }

    private static Vector2 Interpolate(Vector2 last, Vector2 now, float timeStacker) => Vector2.Lerp(last, now, timeStacker);

    private static Vector2 Interpolate(BodyChunk chunk, float timeStacker) => Interpolate(chunk.lastPos, chunk.pos, timeStacker);
}
