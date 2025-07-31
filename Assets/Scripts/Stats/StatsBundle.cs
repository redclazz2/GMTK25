using UnityEngine;

[System.Serializable]
public struct StatsBundle
{
    [Header("Offensive")]
    public float damage;
    public float lifeSteal;
    public float cooldownReduction;

    [Header("Mobility")]
    public float moveSpeed;
    public float size;

    [Header("Survivability")]
    public float maxHealth;
    [HideInInspector] public float health;
    public float regeneration;
    public float shield;

    [Header("Extras")]
    public float armor;
    public float criticalChance;

    // Operador unario para negar stats
    public static StatsBundle operator -(StatsBundle stats)
    {
        return new StatsBundle
        {
            damage = -stats.damage,
            lifeSteal = -stats.lifeSteal,
            cooldownReduction = -stats.cooldownReduction,
            moveSpeed = -stats.moveSpeed,
            size = -stats.size,
            maxHealth = -stats.maxHealth,
            health = -stats.health,
            regeneration = -stats.regeneration,
            shield = -stats.shield,
            armor = -stats.armor,
            criticalChance = -stats.criticalChance
        };
    }

    // Operador para sumar stats
    public static StatsBundle operator +(StatsBundle a, StatsBundle b)
    {
        return new StatsBundle
        {
            damage = a.damage + b.damage,
            lifeSteal = a.lifeSteal + b.lifeSteal,
            cooldownReduction = a.cooldownReduction + b.cooldownReduction,
            moveSpeed = a.moveSpeed + b.moveSpeed,
            size = a.size + b.size,
            maxHealth = a.maxHealth + b.maxHealth,
            health = a.health + b.health,
            regeneration = a.regeneration + b.regeneration,
            shield = a.shield + b.shield,
            armor = a.armor + b.armor,
            criticalChance = a.criticalChance + b.criticalChance
        };
    }

    // Operador para restar stats
    public static StatsBundle operator -(StatsBundle a, StatsBundle b)
    {
        return a + (-b);
    }

    // Operador para multiplicar por un escalar
    public static StatsBundle operator *(StatsBundle stats, float multiplier)
    {
        return new StatsBundle
        {
            damage = stats.damage * multiplier,
            lifeSteal = stats.lifeSteal * multiplier,
            cooldownReduction = stats.cooldownReduction * multiplier,
            moveSpeed = stats.moveSpeed * multiplier,
            size = stats.size * multiplier,
            maxHealth = stats.maxHealth * multiplier,
            health = stats.health * multiplier,
            regeneration = stats.regeneration * multiplier,
            shield = stats.shield * multiplier,
            armor = stats.armor * multiplier,
            criticalChance = stats.criticalChance * multiplier
        };
    }

    // Operador para multiplicar por un escalar (orden inverso)
    public static StatsBundle operator *(float multiplier, StatsBundle stats)
    {
        return stats * multiplier;
    }

    // Método de utilidad para crear stats vacíos
    public static StatsBundle Zero => new();

    // Método de utilidad para verificar si todos los stats son cero
    public readonly bool IsEmpty()
    {
        return damage == 0 && lifeSteal == 0 && cooldownReduction == 0 &&
               moveSpeed == 0 && size == 0 && maxHealth == 0 &&
               health == 0 && regeneration == 0 && shield == 0 &&
               armor == 0 && criticalChance == 0;
    }
}