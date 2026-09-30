using UnityEngine;

namespace AbsoluteFriends.Core;

public readonly struct CreatureCondition(float? health, float? permanentDamage, float? poison, bool? dead)
{
    public float? Health { get; } = health;

    public float? PermanentDamage { get; } = permanentDamage;

    public float? Poison { get; } = poison;

    public bool? Dead { get; } = dead;

    public float? Damage => Health is { } health ? Mathf.Max(1f - health, 0f) : null;

    public static CreatureCondition Read(AbstractCreature creature)
    {
        CreatureState? state = creature.state;

        return new(
            state is HealthState health ? health.health : null,
            state is PlayerState player ? Mathf.Max((float)player.permanentDamageTracking, 0f) : null,
            creature.realizedCreature is { } realized ? Mathf.Max(realized.injectedPoison, 0f) : null,
            state?.dead
        );
    }
}
