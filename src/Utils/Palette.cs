using UnityEngine;

namespace AbsoluteFriends.Utils;

public static class Palette
{
    public static Color Error => new(1f, 0f, 0f);

    public static Color Warning => new(1f, 0.6f, 0.15f);

    public static Color Tracked => new(1f, 0.45f, 1f);

    public static Color Friendly => new(0.45f, 1f, 0.45f);

    public static Color Neutral => new(1f, 1f, 1f);

    public static Color Indirect => new(0.45f, 0.8f, 1f);

    public static Color Owned => new(1f, 0.85f, 0.35f);

    public static Color Unfriendly => new(1f, 0.4f, 0.4f);
}
