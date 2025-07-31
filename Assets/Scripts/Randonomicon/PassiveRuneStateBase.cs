using UnityEngine;

public abstract class PassiveRuneStateBase : IRuneState
{
    private readonly StatsBundle? _statDelta;
    protected GameObject owner;

    protected PassiveRuneStateBase(StatsBundle statDelta)
    {
        _statDelta = statDelta;
    }

    public virtual void Enter(GameObject owner)
    {
        this.owner = owner;
        if (_statDelta != null)
            owner.GetComponent<StatsComponent>().ApplyModifiers(_statDelta.Value);
    }

    public virtual void Tick(float dt)
    {
        // No periodic logic by default
    }

    public virtual void Exit()
    {
        if (_statDelta != null)
            owner.GetComponent<StatsComponent>()
                 .ApplyModifiers(-_statDelta.Value);
    }
}