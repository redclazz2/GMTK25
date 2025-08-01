using UnityEngine;

public class RuneFireball : ActiveRuneStateBase
{
    private readonly float detectionRadius = 5f;

    public RuneFireball(RuneStateData runeStateData) : base(runeStateData, 3)
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
        GameObject runeInstance = GameObject.Instantiate(_stateData.GetPrefab("fireball01"), spawnPos, spawnRot);
        FireballDamage fireball = runeInstance.GetComponent<FireballDamage>();
        fireball.owner = owner;
        fireball.damage = StatsComponent.Get(owner).currentStats.damage;
        fireball.criticalChance = StatsComponent.Get(owner).currentStats.criticalChance;
    }
}