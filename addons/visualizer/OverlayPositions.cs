using System.Runtime.CompilerServices;
using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal sealed class OverlayPositions
{
    private readonly ConditionalWeakTable<AbstractPhysicalObject, StrongBox<Vector2>> _lastPlaces = new();

    public bool TryGet(AbstractPhysicalObject abstractPhysicalObject, RoomCamera camera, Vector2 camPos, float timeStacker, out Vector2 position, out bool isDistant)
    {
        position = default;
        isDistant = true;

        if (abstractPhysicalObject.slatedForDeletion || camera.room?.world is not { } world || camera.room.abstractRoom is not { } viewed)
        {
            return false;
        }

        if (TryGetWorldPosition(abstractPhysicalObject, world, timeStacker, out Vector2 inWorld))
        {
            _lastPlaces.GetOrCreateValue(abstractPhysicalObject).Value = inWorld;

            isDistant = abstractPhysicalObject.realizedObject?.room != camera.room;
        }
        else if (_lastPlaces.TryGetValue(abstractPhysicalObject, out StrongBox<Vector2> last))
        {
            inWorld = last.Value;
        }
        else
        {
            return false;
        }

        Vector2 onScreen = inWorld - world.RoomToWorldPos(camPos, viewed.index);

        position = onScreen.ClampToScreen(camera);
        isDistant |= position != onScreen;

        return true;
    }

    private static bool TryGetWorldPosition(AbstractPhysicalObject abstractPhysicalObject, World world, float timeStacker, out Vector2 position)
    {
        if (
            abstractPhysicalObject.realizedObject is { room.abstractRoom: { } current } realizedObject
            && world.GetAbstractRoom(current.index) != null
            && realizedObject.TryGetPosition(timeStacker, out Vector2 inRoom)
        )
        {
            position = world.RoomToWorldPos(inRoom, current.index);

            return true;
        }

        if (world.GetAbstractRoom(abstractPhysicalObject.pos.room) is { } abstractRoom)
        {
            position = world.RoomToWorldPos(abstractPhysicalObject.pos.PositionInRoom(abstractRoom), abstractRoom.index);

            return true;
        }

        position = default;

        return false;
    }
}
