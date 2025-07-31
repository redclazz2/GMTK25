using UnityEngine;

public interface IRuneState
{
    void Enter(GameObject owner);

    void Tick(float dt);

    void Exit();
}