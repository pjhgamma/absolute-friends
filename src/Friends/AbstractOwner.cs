namespace RippleFriends.Friends;

internal class AbstractOwner(PhysicalObject physicalObject) : AbstractCreature(null, null, null, new(), new())
{
    public readonly PhysicalObject PhysicalObject = physicalObject;

    public string Name => PhysicalObject.GetType().Name;
}
