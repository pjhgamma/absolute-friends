using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal sealed class NameTag : ITag
{
    private const float TextSize = 0.75f;

    private readonly ShadowedNode<FLabel> _text = new(Create);

    public NameTag(Color color)
    {
        _text.Front.color = color;
    }

    public IEnumerable<FNode> Nodes => _text.Nodes;

    public string Text
    {
        set
        {
            if (_text.Front.text != value)
            {
                _text.Apply(label => label.text = value);
            }
        }
    }

    public bool IsVisible
    {
        set => _text.IsVisible = value;
    }

    public bool IsDistant
    {
        set => _text.IsDistant = value;
    }

    public void MoveTo(Vector2 position, float gap) => _text.MoveTo(position + new Vector2(0f, gap));

    private static FLabel Create(bool isShadow) => new(RWCustom.Custom.GetDisplayFont(), "")
    {
        anchorX = 0.5f,
        anchorY = 0.5f,
        scale = TextSize,
        color = isShadow ? Color.black : Color.white,
        isVisible = false,
    };
}
