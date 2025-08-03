using UnityEngine;

public class RuneBasicAttack : ActiveRuneStateBase
{
    public RuneBasicAttack(RuneStateData runeStateData) : base(runeStateData, 0.7f)
    {
    }

    protected override void OnTrigger()
    {
        Vector3 spawnPos = GameObject.FindGameObjectWithTag("CastPlayer").transform.position;
        Quaternion spawnRot = Quaternion.identity;
        GameObject runeInstance = GameObject.Instantiate(_stateData.GetPrefab("basic01"), spawnPos, spawnRot);
        MusicManager.Instance.PlayOneShot(_stateData.GetAudioClip("a1"), 1f);
        BasicDamage basic = runeInstance.GetComponent<BasicDamage>();
        basic.owner = owner;
        basic.damage = StatsComponent.Get(owner).currentStats.damage * 0.7f;
        basic.criticalChance = StatsComponent.Get(owner).currentStats.criticalChance;
    }
}
