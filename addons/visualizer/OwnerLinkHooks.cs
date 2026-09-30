namespace AbsoluteFriends.Visualizer;

internal class OwnerLinkHooks : ObjectOverlayHooks<OwnerLinkOverlay>
{
    protected override Configurable<bool>[] Options => [Config.OwnerLink];

    protected override OwnerLinkOverlay Create(PhysicalObject target) => new(target);
}
