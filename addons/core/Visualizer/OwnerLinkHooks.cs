namespace RippleFriends.Core.Visualizer;

internal class OwnerLinkHooks : ObjectOverlayHooks<OwnerLinkOverlay>
{
    protected override Configurable<bool>[] Options => [Config.OwnerLink];

    protected override OwnerLinkOverlay Create(UpdatableAndDeletable target) => new(target);
}
