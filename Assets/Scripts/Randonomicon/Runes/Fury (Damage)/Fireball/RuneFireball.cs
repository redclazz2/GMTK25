using UnityEngine;

public class RuneFireball : ActiveRuneStateBase
{
    public RuneFireball(RuneStateData runeStateData) : base(runeStateData, 3)
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
        GameObject runeInstance = GameObject.Instantiate(_stateData.GetPrefab("fireball01"), spawnPos, spawnRot);
        FireballDamage fireball = runeInstance.GetComponent<FireballDamage>();
        fireball.owner = owner;
        fireball.damage = StatsComponent.Get(owner).currentStats.damage;
        fireball.criticalChance = StatsComponent.Get(owner).currentStats.criticalChance;
    }
}
