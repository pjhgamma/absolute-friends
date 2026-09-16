namespace RippleFriends.Core;

public class AbstractOwner(PhysicalObject physicalObject) : AbstractCreature(null, null, null, new(), new())
{
    public readonly PhysicalObject PhysicalObject = physicalObject;

    public virtual string DisplayName => PhysicalObject.GetType().Name;
}
