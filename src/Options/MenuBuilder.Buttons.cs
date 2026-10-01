using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Options;

public abstract partial class MenuBuilder
{
    public OpSimpleButton? AddSimpleButton(string text, string description, Action action, float span = 1f, Configurable<bool>? requirement = null, bool enabled = true)
    {
        if (!enabled || !BeginElement(span))
        {
            return null;
        }

        OpSimpleButton simpleButton = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new(ElementWidth * span - Gap * 2f, Spacing),
            Translate(text)
        )
        {
            description = Translate(description)
        };

        simpleButton.OnClick += BindButton(simpleButton, action, requirement);

        AddColumn(span);
        AddElements(simpleButton);

        return simpleButton;
    }

    public OpSimpleImageButton? AddSimpleImageButton(string image, string description, Action action, float span = 1f, float? width = null, FLabelAlignment alignment = FLabelAlignment.Right, Configurable<bool>? requirement = null, bool enabled = true)
    {
        if (!enabled || !BeginElement(span))
        {
            return null;
        }

        float spanWidth = ElementWidth * span;
        float buttonWidth = width ?? spanWidth - Gap * 2f;
        float buttonX = _pos.x + alignment switch
        {
            FLabelAlignment.Left => Gap,
            FLabelAlignment.Center => (spanWidth - buttonWidth) * 0.5f,
            _ => spanWidth - buttonWidth - Gap,
        };
        OpSimpleImageButton imageButton = new(
            new Vector2(buttonX, _pos.y - Spacing * 0.5f),
            new Vector2(buttonWidth, Spacing),
            image
        )
        {
            description = Translate(description)
        };

        imageButton.OnClick += BindButton(imageButton, action, requirement);

        AddColumn(span);
        AddElements(imageButton);

        return imageButton;
    }

    public OpHoldButton? AddHoldButton(string text, string description, Action action, float span = 1f, Configurable<bool>? requirement = null, bool enabled = true)
    {
        if (!enabled || !BeginElement(span))
        {
            return null;
        }

        OpHoldButton holdButton = new(
            new Vector2(_pos.x + Gap, _pos.y - Spacing * 0.5f),
            new Vector2(ElementWidth * span - Gap * 2f, Spacing),
            Translate(text)
        )
        {
            description = Translate(description)
        };

        holdButton.OnPressDone += BindButton(holdButton, action, requirement);

        AddColumn(span);
        AddElements(holdButton);

        return holdButton;
    }

    private OnSignalHandler BindButton(UIfocusable button, Action action, Configurable<bool>? requirement)
    {
        Configurable<bool>? addonRequirement = _buttonRequirement;

        bool CanPress() => (addonRequirement == null || IsMasterActive(addonRequirement))
            && (requirement == null || IsMasterActive(requirement));

        if (addonRequirement != null || requirement != null)
        {
            void UpdateButton() => button.greyedOut = !CanPress();

            UpdateButton();
            Watch(UpdateButton);
        }

        return _ =>
        {
            if (!button.greyedOut && CanPress())
            {
                action();
            }
        };
    }
}
