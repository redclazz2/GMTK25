using UnityEngine;

public class RuneChainLightning : ActiveRuneStateBase
{
    public RuneChainLightning(RuneStateData runeStateData) : base(runeStateData, 0.3f)
    {
    }

    protected override bool CanTrigger()
    {
        return true;
    }

    protected override void OnTrigger()
    {
        Vector3 spawnPos = GameObject.FindGameObjectWithTag("CastPlayer").transform.position;
        Quaternion spawnRot = Quaternion.identity;
        GameObject runeInstance = GameObject.Instantiate(_stateData.GetPrefab("bolt01"), spawnPos, spawnRot);
        ChainLightningDamage fireball = runeInstance.GetComponent<ChainLightningDamage>();
        fireball.owner = owner;
        fireball.damage = (float)(StatsComponent.Get(owner).currentStats.damage * 0.3);
        fireball.criticalChance = StatsComponent.Get(owner).currentStats.criticalChance;
    }
}
