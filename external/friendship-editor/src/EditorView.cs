using AbsoluteFriends.Manager;
using Menu.Remix.MixedUI;

namespace AbsoluteFriends.FriendshipEditor;

internal abstract class EditorView(FriendTab tab, RainWorldGame game)
{
    private readonly List<UIelement> _elements = [];

    internal FriendTab Tab { get; } = tab;

    internal abstract string Title { get; }

    internal IEnumerable<UIelement> Elements => _elements;

    protected RainWorldGame Game { get; } = game;

    protected AbstractCreature? Target => Tab.Friend;

    internal void Add(UIelement element)
    {
        Tab.Add(element);
        _elements.Add(element);
    }

    internal void SetShown(bool shown)
    {
        foreach (UIelement element in _elements)
        {
            EditorUi.SetShown(element, shown);
        }
    }

    internal abstract bool IsVisibleFor(AbstractCreature friend);

    internal abstract bool Owns(RuleKey key, AbstractCreature friend);

    internal abstract void Reload();

    internal abstract void Refresh();
}
