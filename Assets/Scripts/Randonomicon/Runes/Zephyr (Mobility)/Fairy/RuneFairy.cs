using UnityEngine;

public class RuneFairy : PassiveRuneStateBase
{
    private StatsBundle movement_delta = new() { };
    private StatsComponent stats;

    public RuneFairy(RuneStateData runeStateData) : base(runeStateData)
    {

    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);
        stats = StatsComponent.Get(owner);
        movement_delta.moveSpeed = stats.currentStats.moveSpeed * 0.5f;
        stats.currentStats += movement_delta;
    }

    public override void Exit()
    {
        base.Exit();
        stats.currentStats -= movement_delta;
    }
}
