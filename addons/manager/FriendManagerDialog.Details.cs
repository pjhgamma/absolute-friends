using AbsoluteFriends.Core;
using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Manager;

internal sealed partial class FriendManagerDialog
{
    private sealed class DetailsTab
    {
        private const float ButtonSize = 24f;

        private const float NameX = 35f;

        private const float ForgetWidth = 120f;

        private const float TitleHeight = 30f;

        private const float FooterHeight = 52f;

        private readonly FriendManagerDialog _dialog;

        private readonly FriendTab _tab;

        private readonly OpImage _icon;

        private readonly OpLabel _name;

        private readonly NicknameTextBox _nickname;

        private readonly OpSimpleImageButton _save;

        private readonly OpSimpleImageButton _clear;

        private readonly OpCheckBox _tracking;

        private readonly OpLabel _trackingLabel;

        private readonly OpLabel _relationshipsTitle;

        private readonly AutoScrollBox _relationships;

        private readonly OpLabel _noRelationships;

        private readonly List<RelationshipRow> _rows = [];

        private readonly OpImage _divider;

        private readonly OpLabel _forgetNote;

        private readonly HoldButton _forget;

        private AbstractCreature? _shownFriend;

        internal DetailsTab(FriendManagerDialog dialog, FriendTab tab)
        {
            _dialog = dialog;
            _tab = tab;

            float width = tab.Size.x;
            float nameY = tab.Size.y - ButtonSize;
            float trackingY = nameY - 36f;
            float relationshipsTitleY = trackingY - 50f;
            float relationshipsHeight = relationshipsTitleY - 8f - FooterHeight;

            _icon = new(new Vector2(ButtonSize * 0.5f, nameY + ButtonSize * 0.5f), "Futile_White")
            {
                anchor = new(0.5f, 0.5f)
            };
            _name = new(new Vector2(NameX, nameY), new Vector2(width - NameX, ButtonSize), string.Empty, FLabelAlignment.Left);
            _nickname = new(new Configurable<string>(string.Empty, new ConfigurableInfo("Edit this friend's name.")), new Vector2(NameX, nameY), width - NameX - ButtonSize * 2f - 12f)
            {
                allowSpace = true,
                maxLength = FriendNicknames.MaxNicknameLength,
                description = Translation.Of("Edit this friend's name.")
            };
            _save = new(new Vector2(width - ButtonSize * 2f - 6f, nameY), new Vector2(ButtonSize, ButtonSize), "Menu_Symbol_CheckBox")
            {
                description = Translation.Of("Save this nickname.")
            };
            _clear = new(new Vector2(width - ButtonSize, nameY), new Vector2(ButtonSize, ButtonSize), "Menu_Symbol_Clear_All")
            {
                description = Translation.Of("Clear this nickname.")
            };
            _tracking = new(new Configurable<bool>(false, new ConfigurableInfo("Track this friend.")), new Vector2(0f, trackingY));
            _trackingLabel = new(new Vector2(32f, trackingY), new Vector2(width - 32f, ButtonSize), Translation.Of("Track this friend"), FLabelAlignment.Left);
            _relationshipsTitle = new(new Vector2(0f, relationshipsTitleY), new Vector2(width, TitleHeight), Translation.Of("Friend Relationships"), FLabelAlignment.Center, true);
            _relationships = new(new Vector2(0f, FooterHeight), new Vector2(width, relationshipsHeight));
            _noRelationships = new(new Vector2(0f, FooterHeight), new Vector2(width, relationshipsHeight), Translation.Of("No other friends"), FLabelAlignment.Center)
            {
                color = MenuColorEffect.rgbMediumGrey
            };
            _divider = new(new Vector2(0f, FooterHeight - 8f), "pixel")
            {
                scale = new(width, 1f),
                color = MenuColorEffect.rgbMediumGrey
            };
            _forgetNote = new(Vector2.zero, new Vector2(width - ForgetWidth - 12f, ButtonSize), Translation.Of("Clears its like toward every slugcat."), FLabelAlignment.Left)
            {
                color = MenuColorEffect.rgbMediumGrey
            };
            _forget = new(new Vector2(width - ForgetWidth, 0f), new Vector2(ForgetWidth, ButtonSize), Translation.Of("Forget Friend"))
            {
                description = Translation.Of("Hold to forget this friend.")
            };

            _save.OnClick += delegate { SetNickname(_nickname.value); };
            _clear.OnClick += delegate { SetNickname(null); };
            _tracking.OnValueUpdate += delegate { SetTracked(_tracking.value == "true"); };
            _forget.Done += Forget;

            foreach (UIelement element in (UIelement[])[_icon, _name, _nickname, _save, _clear, _tracking, _trackingLabel, _relationshipsTitle, _relationships, _noRelationships, _divider, _forgetNote, _forget])
            {
                tab.Add(element);
            }

            tab.FriendChanged += LoadFriend;
            tab.Refresh += Refresh;
        }

        private void LoadFriend(AbstractCreature? friend)
        {
            if (friend is { IsPlayer: false })
            {
                _nickname.value = friend.FriendName;
            }
        }

        private void Refresh()
        {
            AbstractCreature? friend = _tab.Friend;
            bool named = friend is { IsPlayer: false };
            bool forgettable = friend is { IsPlayer: false, HasFriendTracker: true };
            (string symbol, Color color) = friend?.Icon ?? (string.Empty, Color.white);

            SetShown(_icon, friend != null && Futile.atlasManager.DoesContainElementWithName(symbol));
            SetShown(_name, friend is { IsPlayer: true });

            foreach (UIelement element in (UIelement[])[_nickname, _save, _clear])
            {
                SetShown(element, named);
            }

            foreach (UIelement element in (UIelement[])[_tracking, _trackingLabel, _relationshipsTitle, _relationships])
            {
                SetShown(element, friend != null);
            }

            foreach (UIelement element in (UIelement[])[_divider, _forgetNote, _forget])
            {
                SetShown(element, forgettable);
            }

            RefreshRelationships(friend);

            if (friend == null)
            {
                return;
            }

            bool tracked = friend.IsTracked;
            bool blocked = !friend.IsPlayer && !tracked && !friend.IsExcluded;

            if (!_icon.Hidden)
            {
                _icon.sprite.SetElementByName(symbol);
                _icon.color = color;
                FitIcon(_icon);
            }

            _name.text = friend.PlayerName;
            _name.color = friend.ShortcutColor;
            _nickname.colorText = friend.ShortcutColor;
            _tracking.ForceValue(tracked ? "true" : "false");
            _tracking.greyedOut = friend.IsPlayer || blocked;
            _tracking.description = Translation.Of(friend.IsPlayer
                ? "Players are always tracked."
                : blocked
                    ? "Another addon keeps this friend untracked."
                    : tracked ? "Stop tracking this friend. Friendship is kept." : "Track this friend.");
        }

        private void RefreshRelationships(AbstractCreature? friend)
        {
            List<AbstractCreature> others = friend == null ? [] : [.. _dialog._rows.Select(row => row.Creature).Where(other => other != friend)];

            while (_rows.Count < others.Count)
            {
                _rows.Add(new RelationshipRow(_tab, _relationships));
            }

            _relationships.ContentHeight = others.Count * RelationshipRow.Height;

            if (friend != _shownFriend)
            {
                _shownFriend = friend;
                _relationships.ScrollToTop();
            }

            for (int index = 0; index < _rows.Count; index++)
            {
                if (friend != null && index < others.Count)
                {
                    _rows[index].Show(friend, others[index], _relationships.Top - (index + 1) * RelationshipRow.Height);
                }
                else
                {
                    _rows[index].Hide();
                }
            }

            SetShown(_noRelationships, friend != null && others.Count == 0);
        }

        private void SetTracked(bool tracked)
        {
            if (_tab.Friend is not { } friend)
            {
                return;
            }

            friend.SetTracked(tracked);
            _tab.RefreshManager();
            Refresh();
        }

        private void SetNickname(string? value)
        {
            if (_tab.Friend is not { IsPlayer: false } friend)
            {
                return;
            }

            friend.SetNickname(value == friend.DefaultName ? null : value);
            _nickname.value = friend.FriendName;
            _tab.RefreshManager();
        }

        private void Forget()
        {
            if (_tab.Friend is not { IsPlayer: false } friend)
            {
                return;
            }

            friend.Forget();
            _tab.RefreshManager();
            Refresh();
        }

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

    private sealed class NicknameTextBox(ConfigurableBase configurable, Vector2 position, float width) : OpTextBox(configurable, position, width)
    {
        public override bool CurrentlyFocusableNonMouse => false;

        public override string value
        {
            get => _value;
            set
            {
                value ??= string.Empty;

                if (value.Length > maxLength)
                {
                    value = value.Substring(0, maxLength);
                }

                if (_value != value)
                {
                    _value = value;
                    Change();
                }
            }
        }
    }
}
