using UnityEngine;

namespace AbsoluteFriends.Options;

public static class FocusNavigation
{
    public static T? Nearest<T>(T from, Vector2 origin, IEnumerable<T> candidates, Func<T, Vector2> centerOf, Vector2 direction) where T : class
    {
        T? best = null;
        T? aligned = null;
        float bestScore = float.MaxValue;
        float alignedScore = float.MaxValue;

        foreach (var candidate in candidates)
        {
            if (ReferenceEquals(candidate, from))
            {
                continue;
            }

            Vector2 offset = centerOf(candidate) - origin;
            float along = Vector2.Dot(offset, direction);

            if (along <= 1f)
            {
                continue;
            }

            float across = Mathf.Abs(offset.x * direction.y - offset.y * direction.x);
            float score = along + across * 2f;

            if (score < bestScore)
            {
                best = candidate;
                bestScore = score;
            }

            if (across <= along && score < alignedScore)
            {
                aligned = candidate;
                alignedScore = score;
            }
        }

        return aligned ?? best;
    }
}
