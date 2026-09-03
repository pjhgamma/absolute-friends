using MoreSlugcats;
using RippleFriends.Options;
using System.Runtime.CompilerServices;
using Watcher;

namespace RippleFriends.Friends;

internal static class OwnerUtils
{
    private static readonly ConditionalWeakTable<PhysicalObject, AbstractOwner>.CreateValueCallback _createIteratorOwner = physicalObject => new(physicalObject);

    private static ConditionalWeakTable<UpdatableAndDeletable, AbstractCreature> _owners = new();

    private static ConditionalWeakTable<PhysicalObject, AbstractOwner> _iteratorOwners = new();

    public static void ClearOwners()
    {
        _owners = new();
        _iteratorOwners = new();
    }

    extension(PhysicalObject? physicalObject)
    {
        public Creature? Grabber => physicalObject?.grabbedBy is { Count: > 0 } grasps ? grasps[0]?.grabber : (physicalObject as Player)?.onBack;

        private AbstractCreature? IteratorOwner => physicalObject is Oracle or SLOracleSwarmer or HalcyonPearl or Prince
            ? _iteratorOwners.GetValue(physicalObject, _createIteratorOwner)
            : null;

        public void ChainOwner(UpdatableAndDeletable? target)
        {
            if (physicalObject.Owner is { } owner)
            {
                target.SetOwner(owner);
            }
        }
    }

    extension(AbstractCreature? owner)
    {
        public UpdatableAndDeletable? Realized => owner is AbstractOwner abstractOwner ? abstractOwner.PhysicalObject : owner?.realizedCreature;
    }

    extension(Creature? creature)
    {
        public IEnumerable<PhysicalObject> Holding
        {
            get
            {
                foreach (var grasp in creature?.grasps ?? [])
                {
                    if (grasp?.grabbed is { } grabbed)
                    {
                        yield return grabbed;
                    }
                }

                if ((creature as Player)?.slugOnBack?.slugcat is { } slugcat)
                {
                    yield return slugcat;
                }
            }
        }
    }

    extension(UpdatableAndDeletable? source)
    {
        public AbstractCreature? Owner
        {
            get
            {
                if (source == null)
                {
                    return null;
                }

                if (_owners.TryGetValue(source, out AbstractCreature abstractCreature))
                {
                    return abstractCreature;
                }

                if (source is LizardSpit lizardSpit)
                {
                    return lizardSpit.lizard?.abstractCreature;
                }

                if (source is PhysicalObject physicalObject)
                {
                    if (physicalObject.IteratorOwner is { } iteratorOwner)
                    {
                        return iteratorOwner;
                    }
                    if (
                        Config.FriendGrabbed.IsActive
                        && physicalObject.Grabber is { abstractCreature: { } abstractGrabber } grabber
                        && (
                            !Config.FriendGrabbedForce.IsActive
                            || source is not Creature
                            || grabber is not Player player
                            || !player.input[0].pckp
                        )
                    )
                    {
                        return abstractGrabber;
                    }
                }

                return null;
            }
        }

        public AbstractCreature? ExternalOwner => source.Owner is { } owner && !ReferenceEquals(owner.Realized, source) ? owner : null;

        public void SetOwner(AbstractCreature? target = null)
        {
            if (source == null)
            {
                return;
            }

            if (target == null)
            {
                _owners.Remove(source);
            }
            else if (!_owners.TryGetValue(source, out _))
            {
                _owners.Add(source, target);
            }
        }

        public void SetOwner(Creature? target) => source.SetOwner(target?.abstractCreature);
    }

    extension(AbstractPhysicalObject? source)
    {
        public AbstractCreature? Owner => source?.realizedObject.Owner;

        public AbstractCreature? ExternalOwner => source?.realizedObject.ExternalOwner;

        public void SetOwner(AbstractCreature? target = null) => source?.realizedObject.SetOwner(target);

        public void SetOwner(Creature? target) => source.SetOwner(target?.abstractCreature);
    }
}
