using Menu.Remix.MixedUI;
using UnityEngine;

namespace AbsoluteFriends.Manager;

internal sealed class AutoScrollBox : OpScrollBox
{
    internal AutoScrollBox(Vector2 position, Vector2 size) : base(position, size, size.y, false, false, true)
    {
        doesBackBump = false;
        ScrollLocked = true;
    }

    internal float ContentHeight
    {
        get => contentSize;
        set
        {
            SetContentSize(value);
            ScrollLocked = !Overflows;
        }
    }

    internal bool Overflows => contentSize > size.y;

    internal float Top => contentSize;

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);

        if (Overflows)
        {
            _rectSlidebar?.Show();
        }
        else
        {
            _rectSlidebar?.Hide();
        }
    }
}
