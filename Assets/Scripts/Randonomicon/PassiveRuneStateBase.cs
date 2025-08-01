using UnityEngine;

public abstract class PassiveRuneStateBase : IRuneState
{
    private readonly RuneStateData _stateData;
    protected GameObject owner;

    protected PassiveRuneStateBase(RuneStateData runeStateData)
    {
        _stateData = runeStateData;
    }

    public virtual void Enter(GameObject owner)
    {
        this.owner = owner;
        owner.GetComponent<StatsComponent>().ApplyModifiers(_stateData.statDelta);
    }

    public virtual void Tick(float dt) { }

    public virtual void Exit()
    {
        owner.GetComponent<StatsComponent>().ApplyModifiers(-_stateData.statDelta);
    }
}