using UnityEngine;

namespace RippleFriends.Core.Visualizer;

internal sealed class SideTag(Color tint) : ITag
{
    private readonly NameTag _name = new(tint);

    private readonly IconTag _icon = new();

    private bool _isVisible;

    private bool _hasName;

    private bool _hasIcon;

    public IEnumerable<FNode> Nodes => [.. _name.Nodes, .. _icon.Nodes];

    public bool IsVisible
    {
        set
        {
            _isVisible = value;

            Show();
        }
    }

    public bool IsDistant
    {
        set
        {
            _name.IsDistant = value;
            _icon.IsDistant = value;
        }
    }

    public void Clear()
    {
        _hasName = false;
        _hasIcon = false;
    }

    public void SetName(string text, Vector2 position, float gap)
    {
        _hasName = true;
        _name.Text = text;

        _name.MoveTo(position, gap);
    }

    public void SetIcon(string symbolName, Color color, Vector2 position, float gap)
    {
        _hasIcon = true;

        _icon.UpdateSymbol(symbolName, color);
        _icon.MoveTo(position, gap);
    }

    private void Show()
    {
        _name.IsVisible = _isVisible && _hasName;
        _icon.IsVisible = _isVisible && _hasIcon;
    }
}
