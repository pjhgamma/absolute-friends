using UnityEngine;

namespace AbsoluteFriends.Resonance;

internal readonly struct ResonanceProfile(float damage, float permanent, float poison, bool dead)
{
    public readonly float Damage = damage;

    public readonly float PermanentDamage = permanent;

    public readonly float Poison = poison;

    public readonly bool Dead = dead;

    public static ResonanceProfile Zero => new(0f, 0f, 0f, false);

    public override string ToString()
    {
        return $"(damage {Damage:0.00}, permanent {PermanentDamage:0.00}, poison {Poison:0.00}, dead {Dead})";
    }

    public ResonanceProfile Min(ResonanceProfile other)
    {
        return new(
            Mathf.Min(Damage, other.Damage),
            Mathf.Min(PermanentDamage, other.PermanentDamage),
            Mathf.Min(Poison, other.Poison),
            Dead && other.Dead
        );
    }
}
