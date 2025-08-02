using UnityEngine;

public class RuneChainLightning : ActiveRuneStateBase
{
    private readonly float detectionRadius = 5f;
    public RuneChainLightning(RuneStateData runeStateData) : base(runeStateData, 0.2f)
    {
    }

    protected override bool CanTrigger()
    {
        Vector3 playerPos = owner.transform.position;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(playerPos, detectionRadius);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].CompareTag("Enemy"))
            {
                return true;
            }
        }

        return false;
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
