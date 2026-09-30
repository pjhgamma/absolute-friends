using AbsoluteFriends.Options;
using AbsoluteFriends.Utils;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Core.Manager;

internal sealed partial class FriendManagerDialog
{
    private void BuildDetails(Page page, float detailX, float screenY)
    {
        float textX = detailX + 15f;
        float textWidth = DetailWidth - 30f;
        float nameY = screenY - 205f;

        _typeIcon = new(new Vector2(textX + 17f, nameY + 12f), "Futile_White")
        {
            anchor = new(0.5f, 0.5f)
        };

        _ = new UIelementWrapper(_wrapper, _typeIcon);
        _typeIcon.Hide();

        Configurable<bool> tracking = (Configurable<bool>)Activator.CreateInstance(
            typeof(Configurable<bool>), false, new ConfigurableInfo("Track this friend."))!;

        _trackingCheckbox = new(tracking, new Vector2(textX + 40f, nameY - 36f));
        _trackingCheckbox.OnValueUpdate += delegate { SetTracked(_trackingCheckbox.value == "true"); };
        _trackingLabel = new(new Vector2(textX + 72f, nameY - 36f), new Vector2(150f, 24f), string.Empty, FLabelAlignment.Left);
        _ = new UIelementWrapper(_wrapper, _trackingCheckbox);
        _ = new UIelementWrapper(_wrapper, _trackingLabel);

        _nameLabel = AddLabel(page, string.Empty, textX + 40f, nameY - 3f, 225f, FLabelAlignment.Left);

        Configurable<string> nickname = (Configurable<string>)Activator.CreateInstance(
            typeof(Configurable<string>), string.Empty, new ConfigurableInfo("Edit this friend's name."))!;

        _nicknameBox = new NicknameTextBox(nickname, new Vector2(textX + 40f, nameY), 225f)
        {
            allowSpace = true,
            maxLength = FriendDisplayNames.MaxNicknameLength,
            description = Translation.Of("Edit this friend's name.")
        };
        _saveNickname = new(new Vector2(textX + 271f, nameY), new Vector2(24f, 24f), "Menu_Symbol_CheckBox")
        {
            description = Translation.Of("Save this nickname.")
        };
        _clearNickname = new(new Vector2(textX + 301f, nameY), new Vector2(24f, 24f), "Menu_Symbol_Clear_All")
        {
            description = Translation.Of("Clear this nickname.")
        };

        _saveNickname.OnClick += delegate { SaveNickname(); };
        _clearNickname.OnClick += delegate { ClearNickname(); };

        _ = new UIelementWrapper(_wrapper, _nicknameBox);
        _ = new UIelementWrapper(_wrapper, _saveNickname);
        _ = new UIelementWrapper(_wrapper, _clearNickname);

        float detailY = nameY - 74f;

        _locationLabel = AddLabel(page, string.Empty, textX, detailY, textWidth, FLabelAlignment.Left);
        _statusLabel = AddLabel(page, string.Empty, textX, detailY - DetailRowSpacing, textWidth, FLabelAlignment.Left);
    }

    private void SetTracked(bool tracked)
    {
        if (_selectedFriend is not { } friend)
        {
            return;
        }

        friend.SetTracked(tracked);
        RefreshRows();
        RefreshDetails();
    }

    private void SaveNickname()
    {
        if (_selectedFriend is not { IsPlayer: false } friend)
        {
            return;
        }

        string value = _nicknameBox.value;

        friend.SetNickname(value == friend.DefaultName ? null : value);
        SelectFriend(friend);
        RefreshRows();
    }

    private void ClearNickname()
    {
        if (_selectedFriend is not { IsPlayer: false } friend)
        {
            return;
        }

        friend.SetNickname(null);
        SelectFriend(friend);
        RefreshRows();
    }

    private void SelectFriend(AbstractCreature? creature)
    {
        _selectedFriend = creature;

        if (creature == null)
        {
            _nameLabel.label.text = string.Empty;
            _typeIcon.Hide();
            _locationLabel.label.text = string.Empty;
            _statusLabel.label.text = string.Empty;

            _nicknameBox.Hide();
            _saveNickname.Hide();
            _clearNickname.Hide();
            _trackingLabel.Hide();
            _trackingCheckbox.Hide();

            return;
        }

        _trackingLabel.Show();
        _trackingCheckbox.Show();

        if (creature.IsPlayer)
        {
            _nameLabel.label.text = creature.PlayerName;
            _nicknameBox.Hide();
            _saveNickname.Hide();
            _clearNickname.Hide();
        }
        else
        {
            _nameLabel.label.text = string.Empty;
            _nicknameBox.value = creature.FriendName;
            _nicknameBox.Show();
            _saveNickname.Show();
            _clearNickname.Show();
        }

        SnapDetails();
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (_selectedFriend is not { } friend)
        {
            return;
        }

        CreatureCondition condition = CreatureCondition.Read(friend);
        string life = condition.Dead is true ? Translation.Of("Dead") : condition.Dead is false ? Translation.Of("Alive") : Translation.Of("Unknown");
        bool tracked = friend.IsTracked;

        (string symbol, Color color) = friend.Icon;

        if (Futile.atlasManager.DoesContainElementWithName(symbol))
        {
            _typeIcon.sprite.SetElementByName(symbol);
            _typeIcon.color = color;
            FitIcon(_typeIcon);
            _typeIcon.Show();
        }
        else
        {
            _typeIcon.Hide();
        }

        _locationLabel.label.text = $"{Translation.Of("Location")}: {LocationOf(friend)}";
        _statusLabel.label.text = $"{Translation.Of("Status")}: {life}";
        _trackingLabel.text = TrackingLabel(tracked);
        _trackingCheckbox.ForceValue(tracked ? "true" : "false");
        _trackingCheckbox.description = Translation.Of(tracked ? "Stop tracking this friend." : "Track this friend.");
    }

    private void SnapDetails()
    {
        UIelement[] elements = [_typeIcon, _trackingCheckbox, _trackingLabel, _nicknameBox, _saveNickname, _clearNickname];

        foreach (UIelement element in elements)
        {
            element.lastScreenPos = element.ScreenPos;
            element.GrafUpdate(1f);
        }
    }

    private string LocationOf(AbstractCreature creature)
    {
        AbstractRoom? room = CreatureRoom(creature);

        if (creature.InDen)
        {
            return room?.name is { Length: > 0 } name
                ? $"{Translation.Of("In den")}: {name}"
                : Translation.Of("In den");
        }

        if (room?.name is not { Length: > 0 } roomName)
        {
            return Translation.Of("Location unknown");
        }

        return ReferenceEquals(room, RainWorldUtils.MainCamera(_game)?.room?.abstractRoom)
            ? $"{Translation.Of("Current room")}: {roomName}"
            : $"{Translation.Of("Room")}: {roomName}";
    }

    private static string TrackingLabel(bool tracked) => Translation.Of(tracked ? "Tracked" : "Not tracked");

    private sealed class NicknameTextBox(ConfigurableBase configurable, Vector2 position, float width) : OpTextBox(configurable, position, width)
    {
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
