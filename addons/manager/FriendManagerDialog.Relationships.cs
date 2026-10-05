using AbsoluteFriends.Core;
using AbsoluteFriends.Utils;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Manager;

internal sealed partial class FriendManagerDialog
{
    private sealed partial class DetailsTab
    {
        private static string RelationshipLabel(FriendshipType type) => type switch
        {
            FriendshipType.Tracked => "Tracking",
            FriendshipType.Friendly => "Friendly",
            FriendshipType.Neutral => "Neutral",
            FriendshipType.Indirect => "Indirect",
            _ => "Not friends",
        };

        private static Color RelationshipColor(FriendshipType type) => type switch
        {
            FriendshipType.Tracked => Palette.Tracked,
            FriendshipType.Friendly => Palette.Friendly,
            FriendshipType.Neutral => Palette.Neutral,
            FriendshipType.Indirect => Palette.Indirect,
            _ => MenuColorEffect.rgbMediumGrey,
        };

        private sealed class RelationshipRow
        {
            internal const float Height = 30f;

            private const float LabelHeight = 24f;

            private const float NameX = 28f;

            private const float NameWidth = 150f;

            private const float RelationX = 184f;

            private const float RelationWidth = 100f;

            private const float BarX = 290f;

            private const float BarWidth = 150f;

            private const float BarHeight = 6f;

            private const float TickWidth = 2f;

            private const float TickHeight = 12f;

            private const float TickOutline = 1f;

            private const float PercentWidth = 50f;

            private readonly OpImage _icon;

            private readonly OpLabel _name;

            private readonly OpLabel _relation;

            private readonly OpImage _barBack;

            private readonly OpImage _barFill;

            private readonly OpImage _threshold;

            private readonly OpImage _tempTickBack;

            private readonly OpImage _tempTick;

            private readonly OpLabel _percent;

            private readonly float _percentX;

            internal RelationshipRow(FriendTab tab, AutoScrollBox scrollBox)
            {
                _percentX = scrollBox.size.x - 25f - PercentWidth;
                _icon = new(Vector2.zero, "Futile_White")
                {
                    anchor = new(0.5f, 0.5f)
                };
                _name = new(Vector2.zero, new Vector2(NameWidth, LabelHeight), string.Empty, FLabelAlignment.Left);
                _relation = new(Vector2.zero, new Vector2(RelationWidth, LabelHeight), string.Empty, FLabelAlignment.Left);
                _barBack = new(Vector2.zero, "pixel")
                {
                    scale = new(BarWidth, BarHeight),
                    color = MenuColorEffect.rgbDarkGrey
                };
                _barFill = new(Vector2.zero, "pixel")
                {
                    scale = new(0f, BarHeight)
                };
                _threshold = new(Vector2.zero, "pixel")
                {
                    scale = new(1f, BarHeight),
                    color = MenuColorEffect.rgbMediumGrey
                };
                _tempTickBack = new(Vector2.zero, "pixel")
                {
                    scale = new(TickWidth + TickOutline * 2f, TickHeight + TickOutline * 2f),
                    color = Color.black
                };
                _tempTick = new(Vector2.zero, "pixel")
                {
                    scale = new(TickWidth, TickHeight),
                    color = Color.white
                };
                _percent = new(Vector2.zero, new Vector2(PercentWidth, LabelHeight), string.Empty, FLabelAlignment.Right);

                foreach (UIelement element in Elements)
                {
                    tab.Add(scrollBox, element);
                }
            }

            private UIelement[] Elements => [_icon, _name, _relation, _barBack, _barFill, _threshold, _tempTickBack, _tempTick, _percent];

            internal void Show(AbstractCreature friend, AbstractCreature other, float y)
            {
                (string symbol, Color color) = other.Icon;
                bool hasIcon = Futile.atlasManager.DoesContainElementWithName(symbol);
                FriendshipType friendship = friend.FriendshipWith(other);
                SocialMemory.Relationship? relationship = friend.IsPlayer
                    ? MemoryOf(other, friend)
                    : MemoryOf(friend, other) ?? MemoryOf(other, friend);
                float labelY = y + (Height - LabelHeight) * 0.5f;

                Place(_icon, new Vector2(12f, y + Height * 0.5f));
                Place(_name, new Vector2(NameX, labelY));
                Place(_relation, new Vector2(RelationX, labelY));
                Place(_barBack, new Vector2(BarX, y + (Height - BarHeight) * 0.5f));
                Place(_barFill, new Vector2(BarX, y + (Height - BarHeight) * 0.5f));
                Place(_threshold, new Vector2(BarX + BarWidth * FriendUtils.FriendLikeThreshold, y + (Height - BarHeight) * 0.5f));
                Place(_percent, new Vector2(_percentX, labelY));

                SetShown(_icon, hasIcon);
                SetShown(_name, true);
                SetShown(_relation, true);
                SetShown(_barBack, relationship != null);
                SetShown(_barFill, relationship != null);
                SetShown(_threshold, relationship != null);
                SetShown(_tempTickBack, relationship != null);
                SetShown(_tempTick, relationship != null);
                SetShown(_percent, relationship != null);

                if (hasIcon)
                {
                    _icon.sprite.SetElementByName(symbol);
                    _icon.color = color;
                    FitIcon(_icon);
                }

                string name = other.FriendName;

                _name.text = LabelTest.GetWidth(name) > NameWidth ? LabelTest.TrimText(name, NameWidth, true) : name;
                _name.color = other.ShortcutColor;
                _relation.text = Translation.Of(RelationshipLabel(friendship));
                _relation.color = RelationshipColor(friendship);

                if (relationship == null)
                {
                    return;
                }

                float like = Mathf.Clamp(relationship.like, -1f, 1f);
                float tempLike = Mathf.Clamp(relationship.tempLike, -1f, 1f);

                Vector2 tick = new(BarX + Mathf.Clamp(BarWidth * Mathf.Abs(tempLike) - TickWidth * 0.5f, 0f, BarWidth - TickWidth), y + (Height - TickHeight) * 0.5f);

                Place(_tempTickBack, tick - new Vector2(TickOutline, TickOutline));
                Place(_tempTick, tick);
                _barFill.scale = new(BarWidth * Mathf.Abs(like), BarHeight);
                _barFill.color = _relation.color;
                _percent.text = $"{Mathf.RoundToInt(like * 100f)}%";
                _percent.color = _relation.color;
            }

            internal void Hide()
            {
                foreach (UIelement element in Elements)
                {
                    SetShown(element, false);
                }
            }

            private static SocialMemory.Relationship? MemoryOf(AbstractCreature creature, AbstractCreature subject) => creature.state?.socialMemory?.GetRelationship(subject.ID);

            private static void Place(UIelement element, Vector2 position)
            {
                if (element.GetPos() == position)
                {
                    return;
                }

                element.SetPos(position);
                Snap(element);
            }
        }
    }
}
