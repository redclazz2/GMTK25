using UnityEngine;

public class RuneGoldenWand : PassiveRuneStateBase
{

    public RuneGoldenWand(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        this.owner = owner;

        // Obtener las estadísticas actuales
        StatsComponent statsComponent = StatsComponent.Get(owner);
        StatsBundle currentStats = statsComponent.currentStats;

        // Calcular el incremento del 15% para todas las estadísticas
        StatsBundle statBonus = new StatsBundle
        {
            damage = currentStats.damage * 0.15f,
            lifeSteal = 0.15f,
            cooldownReduction = 0.5f,
            moveSpeed = currentStats.moveSpeed * 0.15f,
            size = currentStats.size * 0.5f,
            maxHealth = currentStats.maxHealth * 0.15f,
            health = 0f, // No aumentamos la vida actual para evitar curación instantánea
            regeneration = 1f,
            armor = -20,
            criticalChance = 0.15f
        };

        // Aplicar las mejoras de estadísticas junto con los modificadores base
        StatsBundle totalModifiers = _stateData.statDelta;
        totalModifiers.damage += statBonus.damage;
        totalModifiers.lifeSteal += statBonus.lifeSteal;
        totalModifiers.cooldownReduction += statBonus.cooldownReduction;
        totalModifiers.moveSpeed += statBonus.moveSpeed;
        totalModifiers.size += statBonus.size;
        totalModifiers.maxHealth += statBonus.maxHealth;
        totalModifiers.health += statBonus.health;
        totalModifiers.regeneration += statBonus.regeneration;
        totalModifiers.armor += statBonus.armor;
        totalModifiers.criticalChance += statBonus.criticalChance;

        statsComponent.ApplyModifiers(totalModifiers);

        Debug.Log($"Berserker Rage activated! All stats increased by 15%, but damage taken increased by 20%");
    }

}