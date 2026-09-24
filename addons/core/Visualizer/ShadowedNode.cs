using UnityEngine;

namespace AbsoluteFriends.Core.Visualizer;

internal sealed class ShadowedNode<TNode>(Func<bool, TNode> create)
    where TNode : FFacetElementNode
{
    private static readonly Vector2 _offset = new(1f, -1f);

    private readonly TNode _lower = create(true);

    private readonly TNode _upper = create(true);

    private readonly TNode _front = create(false);

    private bool _isDistant;

    public TNode Front => _front;

    public IEnumerable<FNode> Nodes => [_lower, _upper, _front];

    public bool IsVisible
    {
        set
        {
            _lower.isVisible = value;
            _upper.isVisible = value;
            _front.isVisible = value;
        }
    }

    public bool IsDistant
    {
        set
        {
            if (_isDistant != value)
            {
                _isDistant = value;
                _front.IsHologram = value;
            }
        }
    }

    public void Apply(Action<TNode> apply)
    {
        apply(_lower);
        apply(_upper);
        apply(_front);
    }

    public void MoveTo(Vector2 position)
    {
        _lower.SetPosition(position + _offset);
        _upper.SetPosition(position - _offset);
        _front.SetPosition(position);
    }
}
