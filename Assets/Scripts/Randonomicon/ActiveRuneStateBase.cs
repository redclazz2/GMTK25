using UnityEngine;

public abstract class ActiveRuneStateBase : IRuneState
{
    private readonly RuneStateData _stateData;
    protected GameObject owner;
    private readonly float _cooldownTime;
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

    protected abstract bool CanTrigger();

    protected abstract void OnTrigger();
}