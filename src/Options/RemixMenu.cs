using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace RippleFriends.Options;

internal abstract class RemixMenuBuilder : OptionInterface
{
    private const float _padding = 30f;
    private const float _spacing = 25f;
    private const float _gap = 5f;
    private readonly Vector2 MarginX = new(_padding, 600f - _padding);
    private float Width => MarginX.y - MarginX.x;

    private OpTab? _currentTab;
    private Vector2 _pos = new();

    private int _columns = 4;
    private float _currentColumn = 0f;
    private float ElementWidth => Width / _columns;

    protected void SetCurrentTab(OpTab? tab)
    {
        _currentTab = tab;
        _pos = new(MarginX.x, 600f);
        _currentColumn = 0;

        AddNewLine();
    }

    protected void AddNewLine(float spacingModifier = 1f)
    {
        _pos.x = MarginX.x;
        _pos.y -= spacingModifier * _spacing;
        _currentColumn = 0;
    }

    protected void ResetColumn()
    {
        if (_currentColumn > 0)
        {
            AddNewLine(1.5f);
        }
    }

    protected void SetColumns(int columns)
    {
        ResetColumn();

        _columns = columns;
    }

    protected void AddLabel(string text, FLabelAlignment alignment = FLabelAlignment.Left, bool bigText = false)
    {
        if (_currentTab == null)
        {
            return;
        }

        ResetColumn();

        float height = (bigText ? 1.5f : 1f) * _spacing;

        if (bigText)
        {
            AddNewLine(0.5f);
        }

        OpLabel label = new(
            new(_pos.x, _pos.y - 6f),
            new(Width, height),
            Translate(text),
            alignment,
            bigText
        )
        {
            autoWrap = true,
        };

        _currentTab.AddItems(label);
        AddNewLine();
    }

    protected void AddTitle(string? title = null, string? description = null, FLabelAlignment alignment = FLabelAlignment.Center)
    {
        ResetColumn();
        AddNewLine();

        if (title != null)
        {
            AddLabel(title, alignment, true);
        }
        if (description != null)
        {
            AddLabel(description, alignment);
        }
    }

    protected OpCheckBox? AddCheckBox(Configurable<bool> configurable, string? text = null, OpCheckBox? master = null)
    {
        if (_currentTab == null)
        {
            return null;
        }

        string desc = Translate(configurable.info?.description ?? "");

        OpLabel label = new(
            new Vector2(_pos.x, _pos.y - _spacing * 0.5f),
            new(ElementWidth - _spacing - _gap, _spacing),
            Translate(text ?? ""),
            FLabelAlignment.Right
        )
        {
            description = desc
        };
        OpCheckBox checkBox = new(
            configurable,
            new Vector2(_pos.x + ElementWidth - _spacing, _pos.y - _spacing * 0.5f)
        )
        {
            description = desc
        };

        if (master != null)
        {
            checkBox.greyedOut = !master.GetValueBool();
            master.OnChange += delegate
            {
                checkBox.greyedOut = !master.GetValueBool();
            };
        }

        if (text != null)
        {
            _currentTab.AddItems(label);
        }
        _currentTab.AddItems(checkBox);

        _pos.x += ElementWidth;
        if ((_currentColumn += 1) > _columns - 1)
        {
            AddNewLine(1.5f);
        }

        return checkBox;
    }

    protected void AddFloatSlider(Configurable<float> configurable, float span = 1, float min = 0f, float max = 5f, string? text = null, OpCheckBox? master = null)
    {
        if (_currentTab == null)
        {
            return;
        }

        if (_currentColumn > _columns - span + 0.5f)
        {
            ResetColumn();
        }

        string desc = Translate(configurable.info?.description ?? "");

        OpLabel label = new(
            new Vector2(_pos.x, _pos.y - _spacing * 0.5f),
            new(ElementWidth - _spacing - _gap, _spacing),
            Translate(text ?? ""),
            FLabelAlignment.Right
        )
        {
            description = desc
        };
        OpFloatSlider slider = new(
            configurable,
            new Vector2(_pos.x + _gap + (text != null ? ElementWidth : 0), _pos.y - _spacing * 0.5f - 3f),
            (int)(ElementWidth * span - _gap * 2f - (text != null ? ElementWidth - _spacing : 0))
        )
        {
            description = desc,
            min = min,
            max = max,
        };

        if (master != null)
        {
            slider.greyedOut = !master.GetValueBool();
            master.OnChange += delegate
            {
                slider.greyedOut = !master.GetValueBool();
            };
        }

        if (text != null)
        {
            _currentTab.AddItems(label);
        }
        _currentTab.AddItems(slider);

        _pos.x += ElementWidth * span;
        if ((_currentColumn += span) > _columns - 1)
        {
            AddNewLine(1.5f);
        }
    }
}

internal class RemixMenu : RemixMenuBuilder
{
    public static readonly RemixMenu Instance = new();

    public override void Initialize()
    {
        base.Initialize();

        List<OpTab> enabledTabs = [];
        OpTab generalTab = new(this, Translate("General"));
        OpTab vanillaTab = new(this, Translate("Vanilla"));
        OpTab? downpourTab = null;
        OpTab? watcherTab = null;

        enabledTabs.Add(generalTab);
        enabledTabs.Add(vanillaTab);
        if (ModManager.MSC)
        {
            downpourTab = new(this, Translate("Downpour"));
            enabledTabs.Add(downpourTab);
        }
        if (ModManager.Watcher)
        {
            watcherTab = new(this, Translate("Watcher"));
            enabledTabs.Add(watcherTab);
        }
        Tabs = [.. enabledTabs];

        SetColumns(4);

        SetCurrentTab(generalTab);

        AddLabel("Ripple Friends", FLabelAlignment.Center, bigText: true);
        AddLabel("Ripple friends do not affect each other.");
        AddLabel("This option itself does nothing, but targets to be affected by the other options.");
        AddLabel("The Ripple Friends relationship applies bidirectionally, excluding oneself.");
        AddCheckBox(Config.FriendPlayer, "Player");
        AddCheckBox(Config.FriendCreature, "Friendly Creatures");
        AddCheckBox(Config.FriendNeutralCreature, "Neutral Creatures");
        AddCheckBox(Config.FriendIterator, "Iterators");
        AddCheckBox(Config.FriendGrabbed, "Grabbed Objects");
        AddCheckBox(Config.FriendArena, "Arena");

        AddTitle("General");
        AddCheckBox(Config.Collision, "Collisions");
        AddCheckBox(Config.Weapon, "Weapons");
        AddCheckBox(Config.Explosion, "Explosions");

        SetCurrentTab(vanillaTab);

        AddTitle("Player Actions");
        var GrabPlayerCheckBox = AddCheckBox(Config.GrabPlayer, "Grab Player");
        AddFloatSlider(Config.GrabPlayerTime, span: 3f, master: GrabPlayerCheckBox);
        AddCheckBox(Config.NoStealing, "No Stealing");
        AddCheckBox(Config.Pebbles, "Pebbles");
        AddCheckBox(Config.Moon, "Moon");
        AddCheckBox(Config.Mushroom, "Mushroom");

        AddTitle("Interactions");
        AddCheckBox(Config.FirecrackerPlant, "Cherrybomb");
        AddCheckBox(Config.Bee, "Beehive");
        AddCheckBox(Config.JellyFish, "Jellyfish");
        AddCheckBox(Config.Snail, "Snail");
        AddCheckBox(Config.TubeWorm, "Grappling Worm");

        if (ModManager.MSC)
        {
            SetCurrentTab(downpourTab);

            AddTitle("Player Actions");
            AddCheckBox(Config.GourmandSlam, "Gourmand Slam");
            AddCheckBox(Config.ArtificerParry, "Artificer Parry");
            AddCheckBox(Config.SaintTongue, "Saint Tongue");
            AddCheckBox(Config.SaintAttunement, "Saint Attunement");

            AddTitle("Interactions");
            AddCheckBox(Config.FireEgg, "Fire Egg");
            AddCheckBox(Config.SingularityBomb, "Singularity Bomb");
        }

        if (ModManager.Watcher)
        {
            SetCurrentTab(watcherTab);

            AddTitle("Interactions");
            AddCheckBox(Config.Pomegranate, "Pomegranate");
            AddCheckBox(Config.Frog, "Frog");
        }
    }
}
