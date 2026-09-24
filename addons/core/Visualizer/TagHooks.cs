namespace AbsoluteFriends.Core.Visualizer;

internal class TagHooks : RoomOverlayHooks<TagOverlay>
{
    protected override Configurable<bool>[] Options => [Config.FriendName, Config.FriendIcon, Config.OwnerName, Config.OwnerIcon];

    protected override string? Subject => "Tag";

    protected override TagOverlay Create(Room room) => new();
}
