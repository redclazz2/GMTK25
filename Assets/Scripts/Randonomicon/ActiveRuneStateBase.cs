using UnityEngine;

public abstract class ActiveRuneStateBase : IRuneState
{
    protected GameObject owner;
    private readonly StatsBundle? _statDelta;
    private readonly float _cooldownTime;
    private float _cooldownRemaining;

    protected ActiveRuneStateBase(StatsBundle statDelta, float cooldownSeconds)
    {
        _statDelta = statDelta;
        _cooldownTime = cooldownSeconds;
        _cooldownRemaining = 0f;
    }

    public virtual void Enter(GameObject owner)
    {
        this.owner = owner;
        if (_statDelta != null)
            owner.GetComponent<StatsComponent>().ApplyModifiers(_statDelta.Value);
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
        if (_statDelta != null)
            owner.GetComponent<StatsComponent>()
                 .ApplyModifiers(-_statDelta.Value);
    }

    protected abstract bool CanTrigger();

    protected abstract void OnTrigger();
}