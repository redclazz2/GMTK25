using UnityEngine;

public abstract class ActiveRuneStateBase : IRuneState
{
    protected readonly RuneStateData _stateData;
    protected GameObject owner;
    protected readonly float _baseCooldownTime;
    protected float _cooldownRemaining;

    protected ActiveRuneStateBase(RuneStateData runeStateData, float cooldownSeconds)
    {
        _stateData = runeStateData;
        _baseCooldownTime = cooldownSeconds;
        _cooldownRemaining = 0f;
    }

    public virtual void Enter(GameObject owner)
    {
        this.owner = owner;
        owner.GetComponent<StatsComponent>().ApplyModifiers(_stateData.statDelta);
    }

    public virtual void Tick(float dt)
    {
        // Reduce cooldown normally
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= dt;

        // If ready and conditions met, trigger
        if (_cooldownRemaining <= 0f && CanTrigger())
        {
            OnTrigger();
            // Apply flat CDR to the reset cooldown
            float currentCDR = StatsComponent.Get(owner).currentStats.cooldownReduction;
            _cooldownRemaining = Mathf.Max(0.1f, _baseCooldownTime - currentCDR);
        }
    }

    public virtual void Exit()
    {
        owner.GetComponent<StatsComponent>().ApplyModifiers(-_stateData.statDelta);
    }

    protected virtual bool CanTrigger()
    {
        Vector3 playerPos = owner.transform.position;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(playerPos, 1f);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].CompareTag("Enemy"))
            {
                return true;
            }
        }

        return false;
    }

    protected abstract void OnTrigger();
}