using AbsoluteFriends.Utils;

namespace AbsoluteFriends.Core.Visualizer;

internal sealed class SubjectTag : ITag
{
    public SideTag Friend { get; } = new(Palette.Primary);

    public SideTag Owner { get; } = new(Palette.Secondary);

    public IEnumerable<FNode> Nodes => [.. Friend.Nodes, .. Owner.Nodes];

    public bool IsVisible
    {
        set
        {
            Friend.IsVisible = value;
            Owner.IsVisible = value;
        }
    }

    public bool IsDistant
    {
        set
        {
            Friend.IsDistant = value;
            Owner.IsDistant = value;
        }
    }

    public void Clear()
    {
        Friend.Clear();
        Owner.Clear();
    }
}
