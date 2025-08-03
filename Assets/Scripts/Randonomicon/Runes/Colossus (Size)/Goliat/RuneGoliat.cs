using UnityEngine;

public class RuneGoliat : PassiveRuneStateBase
{
    private float maxHealthDelta;
    private float sizeDelta;
    private float damageDelta;
    private float critChanceDelta;
    private StatsBundle allStatsDelta;

    public RuneGoliat(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);
        StatsComponent stats = StatsComponent.Get(owner);
        maxHealthDelta = stats.currentStats.maxHealth;
        sizeDelta = stats.currentStats.size * 2f;
        damageDelta = stats.currentStats.damage * 0.5f;
        critChanceDelta = 0.25f;
        allStatsDelta = new StatsBundle
        {
            maxHealth = maxHealthDelta,
            health = maxHealthDelta,
            size = sizeDelta,
            damage = critChanceDelta,
            criticalChance = critChanceDelta,
        };
        stats.ApplyModifiers(allStatsDelta);
    }

    public override void Exit()
    {
        base.Exit();
        StatsComponent stats = StatsComponent.Get(owner);
        allStatsDelta.health = 0;
        stats.ApplyModifiers(-allStatsDelta);
    }
}
