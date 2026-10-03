namespace AbsoluteFriends.Visualizer;

internal abstract class HudOverlay : CosmeticSprite
{
    protected FContainer Container { get; } = new();

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [];
        sLeaser.containers = OverlayUtils.ShouldDraw(rCam, room) ? [Container] : [];

        if (sLeaser.containers.Length > 0)
        {
            AddToContainer(sLeaser, rCam, rCam.HudContainer);
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (OverlayUtils.ShouldDraw(rCam, room))
        {
            (newContatiner ?? rCam.HudContainer).AddChild(Container);
        }
    }

    protected void AddNodes(IEnumerable<FNode> nodes)
    {
        foreach (var node in nodes)
        {
            Container.AddChild(node);
        }
    }
}
