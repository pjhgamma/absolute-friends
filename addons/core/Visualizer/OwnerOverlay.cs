namespace RippleFriends.Core.Visualizer;

internal abstract class OwnerOverlay(UpdatableAndDeletable target) : TargetOverlay(target)
{
    protected AbstractCreature? Owner { get; private set; }

    protected UpdatableAndDeletable? OwnerObject => Owner.RealizedOwner;

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
