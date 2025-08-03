using System.Collections.Generic;
using UnityEngine;

public class RuneComponent : MonoBehaviour
{
    private readonly List<IRuneState> _activeStates = new();
    private IRuneState _basicAttack;
    public RuneStateData initialRune;

    private void Start()
    {
        _basicAttack = RuneFactory.Create(initialRune);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Tick basic attack state if it exists
        _basicAttack?.Tick(dt);

        // Tick all other active states
        for (int i = 0; i < _activeStates.Count; i++)
            _activeStates[i].Tick(dt);
    }

    public void SetBasicAttack(IRuneState basicAttackState)
    {
        if (basicAttackState == null) return;

        // Exit previous basic attack if it exists
        if (_basicAttack != null)
        {
            _basicAttack.Exit();
        }

        // Set and enter the new basic attack state
        _basicAttack = basicAttackState;
        _basicAttack.Enter(gameObject);
    }

    public void RemoveBasicAttack()
    {
        if (_basicAttack != null)
        {
            _basicAttack.Exit();
            _basicAttack = null;
        }
    }

    public bool HasBasicAttack()
    {
        return _basicAttack != null;
    }

    public IRuneState GetBasicAttack()
    {
        return _basicAttack;
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
        // Note: Basic attack is NOT cleared by this method
    }

    public void ClearAllIncludingBasicAttack()
    {
        // Clear regular states
        foreach (var state in _activeStates)
            state.Exit();
        _activeStates.Clear();

        // Clear basic attack
        RemoveBasicAttack();
    }

    public int ActiveStateCount => _activeStates.Count;

    public int TotalActiveStateCount => _activeStates.Count + (_basicAttack != null ? 1 : 0);

    public bool HasState(IRuneState state)
    {
        return _activeStates.Contains(state) || _basicAttack == state;
    }
}