using UnityEngine;

public abstract class ActiveRuneStateBase : IRuneState
{
    protected readonly RuneStateData _stateData;
    protected GameObject owner;
    private float _cooldownTime;
    private float _cooldownRemaining;

    protected ActiveRuneStateBase(RuneStateData runeStateData, float cooldownSeconds)
    {
        _stateData = runeStateData;
        _cooldownTime = cooldownSeconds;
        _cooldownRemaining = 0f;
    }

    public virtual void Enter(GameObject owner)
    {
        this.owner = owner;
        owner.GetComponent<StatsComponent>().ApplyModifiers(_stateData.statDelta);
    }

    public virtual void Tick(float dt)
    {
        // Check cooldown changes
        _cooldownTime -= StatsComponent.Get(owner).currentStats.cooldownReduction;

        // Reduce cooldown
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= dt;

        // If ready and conditions met, trigger
        if (_cooldownRemaining <= 0f && CanTrigger())
        {
            OnTrigger();
            _cooldownRemaining = _cooldownTime;
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