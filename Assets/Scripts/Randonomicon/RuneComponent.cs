using System.Collections.Generic;
using UnityEngine;

public class RuneComponent : MonoBehaviour
{
    private readonly List<IRuneState> _activeStates = new();

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _activeStates.Count; i++)
            _activeStates[i].Tick(dt);
    }

    public void AddState(IRuneState state)
    {
        if (state == null) return;
        if (_activeStates.Contains(state)) return;

        state.Enter(gameObject);
        _activeStates.Add(state);
    }

    public void RemoveState(IRuneState state)
    {
        if (_activeStates.Remove(state))
            state.Exit();
    }

    public void ClearAll()
    {
        foreach (var state in _activeStates)
            state.Exit();
        _activeStates.Clear();
    }

    public int ActiveStateCount => _activeStates.Count;

    public bool HasState(IRuneState state)
    {
        return _activeStates.Contains(state);
    }
}