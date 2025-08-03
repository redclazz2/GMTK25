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

        StatsComponent stats = StatsComponent.Get(owner);
        float baseSizeScale = stats.currentStats.size;

        // Calculate size reduction
        size_delta = baseSizeScale * sizeReduction;

        ApplySizeAndDamageChanges();
    }

    private void ApplySizeAndDamageChanges()
    {
        StatsComponent stats = StatsComponent.Get(owner);

        Debug.Log("SIZE DELTA:" + size_delta);

        // Get base values before any changes
        float baseSizeScale = stats.currentStats.size;
        float baseDamage = stats.currentStats.damage;

        // Apply size reduction ONCE
        stats.currentStats.size -= size_delta;
        float newSize = stats.currentStats.size; // This is the final size after reduction

        // Calculate damage bonus based on size reduction
        float sizeRatio = newSize / baseSizeScale; // Will be < 1 when size is reduced
        float inverseRatio = 1f / sizeRatio; // > 1 when size is reduced

        // Calculate damage multiplier (limit the maximum)
        float damageMultiplier = Mathf.Min(inverseRatio, maxDamageMultiplier);

        // The bonus is the difference from base damage
        newDamageBonus = baseDamage * (damageMultiplier - 1f);
        stats.currentStats.damage += newDamageBonus;

        Debug.Log($"[RuneGiantSlayer] Base Size: {baseSizeScale:F2} -> New Size: {newSize:F2} (ratio: {sizeRatio:F2}), Damage multiplier: {damageMultiplier:F2}x, Damage bonus: {newDamageBonus:F2}");
    }

    public override void Exit()
    {
        base.Exit();
        StatsComponent stats = StatsComponent.Get(owner);
        stats.currentStats.size += size_delta;
        stats.currentStats.damage -= newDamageBonus;
    }
}