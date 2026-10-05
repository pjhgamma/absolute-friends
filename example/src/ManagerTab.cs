using AbsoluteFriends.Core;
using AbsoluteFriends.Manager;
using AbsoluteFriends.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriendsExample;

// The manager builds a new tab every time it opens, so one ManagerTab instance belongs to one open manager.
// The tab's events keep the instance alive while the manager is open; nothing static refers to it, so it is released with the closed manager.
internal sealed class ManagerTab
{
    // The ID identifies this addon's tab. Registering the same ID again replaces the tab, so each addon owns one tab.
    private const string TabId = Plugin.GUID;

    // FriendStorage keeps string values per friend and per story save; prefix keys with your addon's ID so they do not collide.
    // Values are written to the death-persistent save at once, so they survive deaths and quitting. Arena and Safari keep them only for the session.
    // Forgetting a friend in the manager also clears its stored values.
    private const string FavoriteKey = Plugin.GUID + ".favorite";

    private readonly FriendTab _tab;

    private readonly OpImage _icon;

    private readonly OpLabel _infoLabel;

    private readonly OpCheckBox _favoriteBox;

    private float _spin;

    private float _lastSpin;

    // The constructor runs once each time the manager opens, before any friend is selected.
    // The tab is an empty area of tab.Size inside the panel's padding; positions passed to tab.Add are relative to its bottom-left corner.
    // tab.Add moves each element by tab.Origin once. To move an element later with SetPos, add tab.Origin to the position yourself.
    // Any MixedUI element works, including your own UIelement subclasses. Stay inside tab.Size, because nothing is clipped.
    // The manager plays no sounds. There is no menu microphone in game, so elements that need one for looping sounds, such as OpHoldButton, do not work here.
    // An exception from the constructor or any event handler disables only this tab for the open dialog and is logged.
    // Show or hide your own elements only while the tab is active, for example in Refresh or Shown; the manager restores what was visible when the tab returns.
    private ManagerTab(FriendTab tab)
    {
        _tab = tab;

        // A centered OpImage draws the creature icon exactly like the manager's Friend Info tab.
        // For drawing that MixedUI cannot do, add Futile sprites to tab.Container, which sits at the tab's origin and hides with the tab.
        _icon = new OpImage(new Vector2(12f, tab.Size.y - 12f), "Futile_White")
        {
            anchor = new Vector2(0.5f, 0.5f)
        };
        tab.Add(_icon);

        // The icon and this label line up with the icon and name box of the manager's Friend Info tab.
        _infoLabel = new OpLabel(new Vector2(40f, tab.Size.y - 24f), new Vector2(tab.Size.x - 40f, 24f), string.Empty, FLabelAlignment.Left);
        tab.Add(_infoLabel);

        // Value controls need a Configurable. An unbound one holds the value without saving it anywhere.
        // Setting a value to null removes it, so an unchecked box leaves nothing behind in the save.
        _favoriteBox = new OpCheckBox(new Configurable<bool>(false), new Vector2(0f, tab.Size.y - 60f))
        {
            description = Translation.Of("Marks this friend as a favorite.")
        };
        _favoriteBox.OnValueUpdate += delegate
        {
            _tab.Friend?.SetFriendData(FavoriteKey, _favoriteBox.value == "true" ? "true" : null);
        };
        tab.Add(_favoriteBox);
        tab.Add(new OpLabel(new Vector2(32f, tab.Size.y - 60f), new Vector2(150f, 24f), Translation.Of("Favorite"), FLabelAlignment.Left));

        OpSimpleButton wave = new(new Vector2(0f, tab.Size.y - 96f), new Vector2(110f, 24f), Translation.Of("Wave"))
        {
            description = Translation.Of("Spins this friend's icon.")
        };
        wave.OnClick += delegate { _spin = 1f; };
        tab.Add(wave);

        // FriendChanged runs when the selection changes, even while the tab is hidden. Load per-friend values into controls here.
        tab.FriendChanged += ShowFriend;

        // Refresh runs a few times per second while the tab is showing. Update live information here.
        tab.Refresh += RefreshInfo;

        // Update runs every frame while the tab is showing, and GrafUpdate every drawn frame with the time stacker for smooth motion.
        tab.Update += () =>
        {
            _lastSpin = _spin;
            _spin = Mathf.Max(0f, _spin - 0.03f);
        };
        tab.GrafUpdate += timeStacker =>
        {
            _icon.sprite.rotation = 360f * Mathf.SmoothStep(0f, 1f, Mathf.Lerp(_lastSpin, _spin, timeStacker));
        };

        // Closed runs when the manager closes. The manager disposes of the tab's elements and container itself,
        // so subscribe only if you hold something else, such as a static reference or your own resources.

        // Call tab.RefreshManager() after changing something the manager itself shows, such as a name rule's result or a tracking rule.
    }

    // Register during Enable. The title is translated by the manager; the last argument decides, per friend, whether the tab appears.
    internal static void Register() => FriendManager.RegisterTab(TabId, "Notes", tab => _ = new ManagerTab(tab), friend => !friend.IsPlayer);

    internal static void Unregister() => FriendManager.UnregisterTab(TabId);

    private void ShowFriend(AbstractCreature? friend)
    {
        if (friend == null)
        {
            return;
        }

        (string symbol, Color color) = friend.Icon;

        if (Futile.atlasManager.DoesContainElementWithName(symbol))
        {
            _icon.sprite.SetElementByName(symbol);

            // Shrink large icons to 24 pixels, the size the manager uses for creature icons.
            Vector2 size = _icon.sprite.element.sourcePixelSize;
            float scale = Mathf.Min(1f, 24f / Mathf.Max(size.x, size.y));

            _icon.scale = new Vector2(scale, scale);
        }

        _icon.color = color;
        _spin = _lastSpin = 0f;

        // Assigning value would run OnValueUpdate and write the value straight back; ForceValue only shows it.
        _favoriteBox.ForceValue(friend.GetFriendData(FavoriteKey) ?? "false");

        RefreshInfo();
    }

    private void RefreshInfo()
    {
        if (_tab.Friend is not { } friend)
        {
            return;
        }

        _infoLabel.text = friend.IsHunterDaddy && friend.realizedCreature is DaddyLongLegs daddy
            ? Translation.Of("Holding with <PLACEHOLDER> tentacles").FillPlaceholders(daddy.tentacles.Count(tentacle => tentacle.grabChunk != null))
            : friend.FriendName;
    }
}
