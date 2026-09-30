using UnityEngine;

namespace AbsoluteFriends.Visualizer;

internal sealed class IconTag : ITag
{
    private const string Placeholder = "Futile_White";

    private const float IconSize = 24f;

    private const float Offset = 4f;

    private readonly ShadowedNode<FSprite> _symbol = new(Create);

    private string _symbolName = Placeholder;

    private bool _isFound;

    private bool _isVisible;

    public IEnumerable<FNode> Nodes => _symbol.Nodes;

    public bool IsVisible
    {
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;

                Show();
            }
        }
    }

    public bool IsDistant
    {
        set => _symbol.IsDistant = value;
    }

    public void UpdateSymbol(string symbolName, Color color)
    {
        _symbol.Front.color = color;

        if (_symbolName == symbolName)
        {
            return;
        }

        _symbolName = symbolName;
        _isFound = Futile.atlasManager.DoesContainElementWithName(symbolName);

        if (_isFound)
        {
            _symbol.Apply(sprite => Reshape(sprite, symbolName));
        }

        Show();
    }

    public void MoveTo(Vector2 position, float gap) => _symbol.MoveTo(position + new Vector2(0f, gap - Offset));

    private static FSprite Create(bool isShadow) => new(Placeholder, true)
    {
        color = isShadow ? Color.black : Color.white,
        isVisible = false,
    };

    private static void Reshape(FSprite sprite, string symbolName)
    {
        sprite.SetElementByName(symbolName);
        sprite.scale = Mathf.Min(1f, IconSize / Mathf.Max(sprite.element.sourcePixelSize.x, sprite.element.sourcePixelSize.y));
    }

    private void Show() => _symbol.IsVisible = _isVisible && _isFound;
}
