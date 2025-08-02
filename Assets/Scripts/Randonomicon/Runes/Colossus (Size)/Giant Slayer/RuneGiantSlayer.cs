using UnityEngine;

public class RuneGiantSlayer : PassiveRuneStateBase
{
    private float size_delta;
    private float newDamageBonus;

    // Configuración de la runa
    private float sizeReduction = 0.3f; // Reduce el tamaño en 30%
    private float maxDamageMultiplier = 1.5f; // Máximo multiplicador de daño

    public RuneGiantSlayer(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);

        size_delta = StatsComponent.Get(owner).currentStats.size * sizeReduction;
        ApplySizeAndDamageChanges();
    }

    private void ApplySizeAndDamageChanges()
    {
        // 
        // Calculamos el bonus de daño inversamente proporcional al tamaño
        // Cuanto más pequeño, más daño
        float baseSizeScale = StatsComponent.Get(owner).currentStats.size;
        StatsComponent.Get(owner).currentStats.size -= size_delta;
        float newSize = StatsComponent.Get(owner).currentStats.size - size_delta;
        float sizeRatio = newSize / baseSizeScale; // Será < 1 cuando el tamaño se reduce
        float inverseRatio = 1f / sizeRatio; // > 1 cuando el tamaño se reduce

        // Calculamos el multiplicador de daño (limitamos el máximo)
        float damageMultiplier = Mathf.Min(inverseRatio, maxDamageMultiplier);

        // El bonus es la diferencia respecto al daño base
        float baseDamage = StatsComponent.Get(owner).currentStats.damage;
        newDamageBonus = baseDamage * (damageMultiplier - 1f);
        StatsComponent.Get(owner).currentStats.damage += newDamageBonus;

        Debug.Log($"[RuneShrink] Size: {newSize:F2} (ratio: {sizeRatio:F2}), Damage multiplier: {damageMultiplier:F2}x");
    }

    public override void Exit()
    {
        base.Exit();
        StatsComponent.Get(owner).currentStats.size += size_delta;
        StatsComponent.Get(owner).currentStats.damage -= newDamageBonus;
    }
}