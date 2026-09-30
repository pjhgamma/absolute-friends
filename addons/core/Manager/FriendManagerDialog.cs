using AbsoluteFriends.Utils;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Core.Manager;

internal sealed partial class FriendManagerDialog : Dialog
{
    private const float RowSpacing = 40f;

    private const float DetailRowSpacing = 30f;

    private const float ViewHeight = 420f;

    private const float ListWidth = 390f;

    private const float DetailWidth = 390f;

    private const float TransitionOffset = ViewHeight + 180f;

    private const float TransitionDuration = 0.3f;

    private const float ShadeAlpha = 0.75f;

    private const float IconSize = 24f;

    private readonly RainWorldGame _game;

    private readonly MenuTabWrapper _wrapper;

    private readonly FContainer _pageContainer;

    private readonly List<FriendRow> _rows = [];

    private readonly MenuLabel _emptyLabel;

    private readonly float _listX;

    private readonly float _screenY;

    private OpScrollBox? _scrollBox;

    private UIelementWrapper? _scrollWrapper;

    private MenuLabel _nameLabel = null!;

    private OpImage _typeIcon = null!;

    private MenuLabel _locationLabel = null!;

    private MenuLabel _statusLabel = null!;

    private NicknameTextBox _nicknameBox = null!;

    private OpSimpleImageButton _saveNickname = null!;

    private OpSimpleImageButton _clearNickname = null!;

    private OpLabel _trackingLabel = null!;

    private OpCheckBox _trackingCheckbox = null!;

    private AbstractCreature? _selectedFriend;

    private int _showDelay = 2;

    private bool _pausedGame;

    private bool _previousPaused;

    private bool _closing;

    private float _nextDetailsRefresh;

    private float _nextMembershipRefresh;

    private float _lastOpenProgress;

    private float _openProgress;

    internal FriendManagerDialog(ProcessManager manager, RainWorldGame game) : base(manager)
    {
        _game = game;
        container.isVisible = false;

        Vector2 screen = manager.rainWorld.options.ScreenSize;
        float center = screen.x * 0.5f;
        float detailX = center + 10f;
        Page page = dialogPage;

        _listX = center - 410f;
        _screenY = screen.y;

        _pageContainer = new()
        {
            y = TransitionOffset
        };
        container.AddChild(_pageContainer);
        page.myContainer = _pageContainer;

        darkSprite.alpha = 0f;
        darkSprite.scaleX = Futile.screen.pixelWidth + 2f;
        darkSprite.scaleY = Futile.screen.pixelHeight + 2f;

        string titleText = Translation.Of("Tracked Friends");
        bool bigTitle = titleText == "Tracked Friends";
        float titleY = screen.y - 82f;
        MenuLabel title = new(this, page, titleText, new Vector2(center - 420f, titleY - 20f), new Vector2(840f, 40f), bigTitle, default);

        title.label.scale = bigTitle ? 1.4f : 1.8f;
        page.subObjects.Add(title);

        AddLabel(page, Translation.Of("Friendship is maintained when tracking is disabled."), center - 420f, screen.y - 135f, 840f);

        _wrapper = new(this, page);
        page.subObjects.Add(_wrapper);

        OpRect detailBorder = new(new Vector2(detailX, screen.y - 568f), new Vector2(DetailWidth, ViewHeight));

        _ = new UIelementWrapper(_wrapper, detailBorder);

        _emptyLabel = AddLabel(page, string.Empty, _listX, screen.y - 360f, ListWidth);

        BuildDetails(page, detailX, screen.y);
        BuildList(CollectCreatures(), 0f);
        _scrollBox?.ScrollToTop(true);
        SelectFriend(_rows.FirstOrDefault()?.Creature);

        _nextMembershipRefresh = Time.unscaledTime + 1f;
    }

    public override void Update()
    {
        PauseGame();

        float previousScroll = _scrollBox?.targetScrollOffset ?? 0f;

        base.Update();

        bool togglePressed = FriendManagerHooks.ConsumeShortcut() || RWInput.CheckPauseButton(0);

        if (togglePressed)
        {
            _closing = true;
        }

        _lastOpenProgress = _openProgress;
        _openProgress = Mathf.Clamp01(_openProgress + (_closing ? -1f : 1f) * Time.unscaledDeltaTime / TransitionDuration);

        if (!_closing && _lastOpenProgress < 1f && _openProgress >= 1f)
        {
            _scrollBox?.ScrollToTop(true);
            _scrollBox?.MarkDirty();
        }

        if (Time.unscaledTime >= _nextMembershipRefresh)
        {
            _nextMembershipRefresh = Time.unscaledTime + 1f;

            RefreshMembership();
        }

        if (Time.unscaledTime >= _nextDetailsRefresh)
        {
            _nextDetailsRefresh = Time.unscaledTime + 0.25f;

            RefreshRows();
            RefreshDetails();
        }

        float wheel = mouseScrollWheelMovement != 0
            ? -mouseScrollWheelMovement * 40f
            : -Input.mouseScrollDelta.y * RowSpacing;

        if (_scrollBox != null && wheel != 0f && !Input.GetMouseButton(0))
        {
            _scrollBox.targetScrollOffset = Mathf.Clamp(previousScroll + wheel, _scrollBox.MaxScroll, 0f);
        }

        if (_showDelay > 0 && --_showDelay == 0)
        {
            container.isVisible = true;
        }

        if (_closing && _openProgress <= 0f)
        {
            manager.StopSideProcess(this);
        }
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);

        float progress = Mathf.Lerp(_lastOpenProgress, _openProgress, timeStacker);
        float eased = Mathf.SmoothStep(0f, 1f, progress);

        _pageContainer.y = TransitionOffset * (1f - eased);
        darkSprite.alpha = ShadeAlpha * eased;
    }

    public override void ShutDownProcess()
    {
        if (_pausedGame)
        {
            _game.paused = _previousPaused;
        }

        base.ShutDownProcess();
    }

    internal void PauseGame()
    {
        if (_pausedGame)
        {
            return;
        }

        _previousPaused = _game.paused;
        _game.paused = true;
        _pausedGame = true;
    }

    private static void FitIcon(OpImage icon)
    {
        Vector2 size = icon.sprite.element.sourcePixelSize;
        float scale = Mathf.Min(1f, IconSize / Mathf.Max(size.x, size.y));

        icon.scale = new(scale, scale);
    }

    private MenuLabel AddLabel(Page page, string text, float x, float y, float width, FLabelAlignment alignment = default)
    {
        float offset = alignment switch
        {
            FLabelAlignment.Left => -width * 0.5f,
            FLabelAlignment.Right => width * 0.5f,
            _ => 0f,
        };

        MenuLabel label = new(this, page, text, new Vector2(x + offset, y), new Vector2(width, 30f), false, default);

        label.label.alignment = alignment;

        page.subObjects.Add(label);

        return label;
    }
}
