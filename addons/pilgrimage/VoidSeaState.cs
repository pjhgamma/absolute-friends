namespace AbsoluteFriends.Pilgrimage;

internal class VoidSeaState
{
    public readonly Dictionary<Creature, HashSet<AbstractCreature>> Owners = [];

    public readonly Dictionary<AbstractCreature, Player> MovedBy = [];

    public readonly Dictionary<Creature, WormThreadOverlay> Threads = [];

    public int Clock = -1;

    public bool RideReleased;

    public bool CanFollow(Creature friend, AbstractCreature abstractPlayer)
    {
        return !Owners.TryGetValue(friend, out HashSet<AbstractCreature> owners) || owners.Count == 0 || owners.Contains(abstractPlayer);
    }
}
