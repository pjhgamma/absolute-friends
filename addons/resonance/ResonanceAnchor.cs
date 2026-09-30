using AbsoluteFriends.Core;
using AbsoluteFriends.Options;

namespace AbsoluteFriends.Resonance;

internal class ResonanceAnchor : UpdatableAndDeletable
{
    public readonly Dictionary<AbstractCreature, int> Grasps = [];

    public HashSet<Player> Players = [];

    public int Reflection;

    public ResonanceAnchor(Room room)
    {
        this.room = room;
    }

    public Func<Creature, bool> Predicate => field ??= creature => creature.room == room;

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!FriendUtils.IsFriendSession)
        {
            return;
        }

        if (Config.ResonanceRoom.IsActive && this.ResonateRoom())
        {
            return;
        }

        if (Config.ResonanceGrab.IsActive)
        {
            this.ResonateGrab();
        }
    }
}
