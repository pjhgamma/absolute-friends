using AbsoluteFriends.Utils;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Core.Manager;

internal sealed partial class FriendManagerDialog
{
    private sealed class FriendRow(AbstractCreature creature, OpLabel name, OpLabel status, OpImage? icon, UIelementWrapper[] wrappers)
    {
        internal AbstractCreature Creature { get; } = creature;

        internal OpLabel Name { get; } = name;

        internal OpLabel Status { get; } = status;

        internal OpImage? Icon { get; } = icon;

        internal UIelementWrapper[] Wrappers { get; } = wrappers;
    }

    private static List<AbstractCreature> CollectCreatures() => [.. FriendUtils.ManageableFriends
        .OrderBy(creature => creature.IsPlayer ? 0 : creature.IsSlugpup ? 1 : 2)
        .ThenBy(creature => creature.creatureTemplate?.name, StringComparer.Ordinal)
        .ThenBy(creature => creature.ID.number)];

    private static float ContentHeight(int count) => Mathf.Max(ViewHeight, count * RowSpacing + 20f);

    private static string RowStatus(AbstractCreature creature) => TrackingLabel(creature.IsTracked);

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
        ClearList();

        float contentHeight = ContentHeight(creatures.Count);
        bool hasSlideBar = contentHeight > ViewHeight;

        _scrollBox = new(new Vector2(_listX, _screenY - 568f), new Vector2(ListWidth, ViewHeight), contentHeight, false, false, hasSlideBar)
        {
            doesBackBump = false
        };
        _scrollWrapper = new(_wrapper, _scrollBox);
        _scrollWrapper.glow?.Hide();

        for (int index = 0; index < creatures.Count; index++)
        {
            AbstractCreature creature = creatures[index];
            float y = contentHeight - 50f - index * RowSpacing;
            (string symbol, Color color) = creature.Icon;
            OpLabel name = new(new Vector2(44f, y + 7f), new Vector2(157f, 30f), creature.FriendName, FLabelAlignment.Left);
            OpLabel status = new(new Vector2(205f, y + 7f), new Vector2(106f, 30f), RowStatus(creature), FLabelAlignment.Right);
            OpSimpleImageButton details = new(new Vector2(317f, y + 10f), new Vector2(24f, 24f), "Menu_InfoI")
            {
                description = Translation.Of("Show this friend's details.")
            };

            details.OnClick += delegate
            {
                SelectFriend(creature);
            };

            List<UIelement> elements = [name, details, status];
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

            _scrollBox.AddItems([.. elements]);

            UIelementWrapper[] wrappers = [.. elements.Select(element => new UIelementWrapper(_wrapper, element))];

            if (icon != null)
            {
                FitIcon(icon);
            }

            _rows.Add(new(creature, name, status, icon, wrappers));
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
        wrapper.glow?.sprite.RemoveFromContainer();
        element.Unload();
    }

    private void RefreshRows()
    {
        foreach (FriendRow row in _rows)
        {
            row.Name.text = row.Creature.FriendName;
            row.Status.text = RowStatus(row.Creature);

            row.Icon?.color = row.Creature.Icon.Color;
        }
    }
}
