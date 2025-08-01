using UnityEngine;

public class RuneArcaneCircle : PassiveRuneStateBase
{
    private GameObject runeInstance;

    public RuneArcaneCircle(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);
        runeInstance = GameObject.Instantiate(_stateData.GetPrefab("circle01"), owner.transform);
        ArcaneCircleDamage arcaneCircle = runeInstance.GetComponent<ArcaneCircleDamage>();
        arcaneCircle.damage = StatsComponent.Get(owner).currentStats.damage;
        arcaneCircle.criticalChance = StatsComponent.Get(owner).currentStats.criticalChance;
    }

    public override void Exit()
    {
        base.Exit();
        GameObject.Destroy(runeInstance);
    }
}