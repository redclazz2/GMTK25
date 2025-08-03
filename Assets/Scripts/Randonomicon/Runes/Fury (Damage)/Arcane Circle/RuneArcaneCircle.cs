using UnityEngine;

public class RuneArcaneCircle : PassiveRuneStateBase
{
    private GameObject runeInstance;
    private ArcaneCircleDamage arcaneCircleComponent;

    public RuneArcaneCircle(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);
        runeInstance = GameObject.Instantiate(_stateData.GetPrefab("circle01"), owner.transform);
        runeInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        arcaneCircleComponent = runeInstance.GetComponent<ArcaneCircleDamage>();
        arcaneCircleComponent.owner = owner;
        // Set initial values
        UpdateDamageStats();
    }

    public override void Tick(float dt)
    {
        base.Tick(dt);
        // Update damage stats every frame to reflect current player stats
        if (arcaneCircleComponent != null)
        {
            UpdateDamageStats();
        }
    }

    private void UpdateDamageStats()
    {
        var stats = StatsComponent.Get(owner);
        arcaneCircleComponent.damage = stats.currentStats.damage;
        arcaneCircleComponent.criticalChance = stats.currentStats.criticalChance;
    }

    public override void Exit()
    {
        base.Exit();
        if (runeInstance != null)
        {
            GameObject.Destroy(runeInstance);
        }
    }
}