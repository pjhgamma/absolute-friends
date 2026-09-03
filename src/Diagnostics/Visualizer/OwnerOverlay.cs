using RippleFriends.Friends;

namespace RippleFriends.Diagnostics.Visualizer;

internal abstract class OwnerOverlay(UpdatableAndDeletable target) : TargetOverlay(target)
{
    protected AbstractCreature? Owner { get; private set; }

    protected UpdatableAndDeletable? OwnerObject => Owner.Realized;

    protected override bool IsVisible => Owner != null;

    protected virtual void OwnerChanged()
    {
    }

    protected override void Follow()
    {
        base.Follow();

        AbstractCreature? owner = Target.ExternalOwner;

        if (!ReferenceEquals(owner, Owner))
        {
            Owner = owner;

            OwnerChanged();
        }
    }
}
