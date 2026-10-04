using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Manager;

internal sealed partial class FriendManagerDialog
{
    private sealed class FriendRow(AbstractCreature creature, RowButton button, RowWrapper focus, OpLabel name, OpLabel status, OpImage? icon, UIelementWrapper[] wrappers)
    {
        internal AbstractCreature Creature { get; } = creature;

        internal RowButton Button { get; } = button;

        internal RowWrapper Focus { get; } = focus;

        internal OpLabel Name { get; } = name;

        internal OpLabel Status { get; } = status;

        internal OpImage? Icon { get; } = icon;

        internal UIelementWrapper[] Wrappers { get; } = wrappers;
    }

    private static List<AbstractCreature> CollectCreatures() => [.. FriendUtils.TrackingCandidates
        .OrderBy(creature => creature.IsPlayer ? 0 : creature.IsSlugpup ? 1 : 2)
        .ThenBy(creature => creature.creatureTemplate?.name, StringComparer.Ordinal)
        .ThenBy(creature => creature.ID.number)];

    private static float ContentHeight(int count) => Mathf.Max(ViewHeight, count * RowSpacing + 20f);

    private static string RowStatus(AbstractCreature creature) => creature.IsPlayer ? string.Empty : TrackingLabel(creature.IsTracked);

    private static AbstractRoom? CreatureRoom(AbstractCreature creature) => creature.realizedCreature?.room?.abstractRoom ?? creature.Room;

    private void RefreshMembership()
    {
        List<AbstractCreature> creatures = CollectCreatures();

        if (_rows.Select(row => row.Creature).SequenceEqual(creatures))
        {
            return;
        }

        AbstractCreature? selected = _selectedFriend;
        float scroll = _scrollBox?.targetScrollOffset ?? 0f;

        BuildList(creatures, scroll);

        if (selected != null && creatures.Contains(selected))
        {
            RefreshDetails();
        }
        else
        {
            SelectFriend(creatures.FirstOrDefault());
        }
    }

    private void BuildList(List<AbstractCreature> creatures, float scroll)
    {
        bool focusedInList = selectedObject is RowWrapper;

        ClearList();

        _contentHeight = ContentHeight(creatures.Count);

        bool hasSlideBar = _contentHeight > ViewHeight;

        _scrollBox = new(new Vector2(_listX, _screenY - 568f), new Vector2(ListWidth, ViewHeight), _contentHeight, false, false, hasSlideBar)
        {
            doesBackBump = false
        };
        _scrollWrapper = new(_wrapper, _scrollBox);
        _scrollWrapper.glow?.Hide();
        _scrollWrapper.page.selectables.Remove(_scrollWrapper);
        _selectionBox = AddSelectionBox();

        for (int index = 0; index < creatures.Count; index++)
        {
            AbstractCreature creature = creatures[index];
            float y = RowY(index);
            (string symbol, Color color) = creature.Icon;
            OpLabel name = new(new Vector2(44f, y + 7f), new Vector2(157f, 30f), creature.FriendName, FLabelAlignment.Left)
            {
                color = creature.ShortcutColor
            };
            OpLabel status = new(new Vector2(205f, y + 7f), new Vector2(136f, 30f), RowStatus(creature), FLabelAlignment.Right);
            RowButton button = new(new Vector2(8f, y + 4f), new Vector2(344f, 36f))
            {
                description = Translation.Of("Show this friend's details.")
            };
            List<UIelement> elements = [name, status];

            button.Clicked += () => ClickRow(creature);
            OpImage? icon = null;

            if (Futile.atlasManager.DoesContainElementWithName(symbol))
            {
                icon = new(new Vector2(24f, y + 22f), symbol)
                {
                    anchor = new(0.5f, 0.5f),
                    color = color
                };

                elements.Add(icon);
            }

            _scrollBox.AddItems([button, .. elements]);

            RowWrapper focus = new(_wrapper, button, _scrollBox);
            UIelementWrapper[] wrappers = [focus, .. elements.Select(element => new UIelementWrapper(_wrapper, element))];

            if (icon != null)
            {
                FitIcon(icon);
            }

            _rows.Add(new(creature, button, focus, name, status, icon, wrappers));
        }

        LinkRows();

        HighlightSelection(true);

        if (focusedInList)
        {
            selectedObject = SelectedRow()?.Focus;
        }

        _scrollBox.targetScrollOffset = Mathf.Clamp(scroll, _scrollBox.MaxScroll, 0f);
        _emptyLabel.label.text = creatures.Count == 0 ? Translation.Of("No trackable friends") : string.Empty;
    }

    private void ClearList()
    {
        foreach (FriendRow row in _rows)
        {
            foreach (UIelementWrapper wrapper in row.Wrappers)
            {
                RemoveElement(wrapper);
            }
        }

        _rows.Clear();

        if (_selectionBox != null && _wrapper.wrappers.TryGetValue(_selectionBox, out UIelementWrapper selectionWrapper))
        {
            RemoveElement(selectionWrapper);
        }

        _selectionBox = null;

        if (_scrollWrapper != null)
        {
            RemoveElement(_scrollWrapper);
            _scrollWrapper = null;
            _scrollBox = null;
        }
    }

    private void RemoveElement(UIelementWrapper wrapper)
    {
        UIelement element = wrapper.thisElement;

        if (element.InScrollBox)
        {
            OpScrollBox.RemoveItemsFromScrollBox([element]);
        }

        _wrapper._tab.RemoveItems([element]);
        _wrapper.wrappers.Remove(element);
        _wrapper.RemoveSubObject(wrapper);
        wrapper.page.selectables.Remove(wrapper);
        wrapper.glow?.sprite.RemoveFromContainer();

        if (selectedObject == wrapper)
        {
            selectedObject = null;
        }

        element.Unload();
    }

    private float RowY(int index) => _contentHeight - 50f - index * RowSpacing;

    private OpRect AddSelectionBox()
    {
        OpRect box = new(new Vector2(8f, 0f), new Vector2(344f, 36f), HighlightAlpha);

        _scrollBox!.AddItems(box);
        _ = new UIelementWrapper(_wrapper, box);
        box.Hide();

        return box;
    }

    private void LinkRows()
    {
        _wrapper.wrappers.TryGetValue(_trackingCheckbox, out UIelementWrapper? details);

        for (int index = 0; index < _rows.Count; index++)
        {
            RowWrapper focus = _rows[index].Focus;

            focus.nextSelectable[0] = focus;
            focus.nextSelectable[1] = _rows[(index + _rows.Count - 1) % _rows.Count].Focus;
            focus.nextSelectable[2] = details;
            focus.nextSelectable[3] = _rows[(index + 1) % _rows.Count].Focus;
        }
    }

    private FriendRow? SelectedRow() => _rows.Find(row => row.Creature == _selectedFriend);

    private void HighlightSelection(bool snap)
    {
        int index = _rows.FindIndex(row => row.Creature == _selectedFriend);

        if (_selectionBox == null)
        {
            return;
        }

        if (index < 0)
        {
            _selectionBox.Hide();

            return;
        }

        _selectionTarget = RowY(index) + 4f;

        if (snap || _selectionBox.Hidden)
        {
            _selectionY = _lastSelectionY = _selectionTarget;
        }

        _selectionBox.Show();
    }

    private void UpdateList()
    {
        _lastSelectionY = _selectionY;
        _selectionY = Mathf.Lerp(_selectionY, _selectionTarget, SelectionEase);

        _lastFocusedRow = manager.menuesMouseMode ? -1 : FollowFocus(_rows.FindIndex(row => row.Button.Focused));
    }

    private int FollowFocus(int focused)
    {
        if (focused < 0)
        {
            return -1;
        }

        FriendRow row = _rows[focused];

        if (_lastFocusedRow < 0 && SelectedRow() is { } current && current != row)
        {
            selectedObject = current.Focus;
            row = current;
        }
        else if (row.Creature != _selectedFriend)
        {
            SelectFriend(row.Creature);
        }

        OpScrollBox.ScrollToChild(row.Button, true);

        return _rows.IndexOf(row);
    }

    private void ClickRow(AbstractCreature creature)
    {
        if (creature == _selectedFriend)
        {
            return;
        }

        PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
        SelectFriend(creature);
    }

    private void PlaceSelection(float timeStacker)
    {
        _selectionBox?.SetPos(new Vector2(8f, Mathf.Lerp(_lastSelectionY, _selectionY, timeStacker)));
    }

    private void RefreshRows()
    {
        foreach (FriendRow row in _rows)
        {
            row.Name.text = row.Creature.FriendName;
            row.Name.color = row.Creature.ShortcutColor;
            row.Status.text = RowStatus(row.Creature);

            row.Icon?.color = row.Creature.Icon.Color;
        }
    }
}
