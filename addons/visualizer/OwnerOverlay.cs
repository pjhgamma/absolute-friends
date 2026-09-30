using AbsoluteFriends.Core;

namespace AbsoluteFriends.Visualizer;

internal abstract class OwnerOverlay(PhysicalObject target) : TargetOverlay(target)
{
    private readonly UpdateTimer _timer = new();

    protected AbstractCreature? Owner { get; private set; }

    protected PhysicalObject? OwnerObject => Owner.RealizedOwner as PhysicalObject;

    protected override bool IsVisible => Owner != null;

    protected virtual void OwnerChanged()
    {
    }

    protected override void Follow()
    {
        base.Follow();

        if (!_timer.Elapse())
        {
            return;
        }

        AbstractCreature? owner = Target.ExternalOwner;

        if (!ReferenceEquals(owner, Owner))
        {
            Owner = owner;

            OwnerChanged();
        }
    }
}
