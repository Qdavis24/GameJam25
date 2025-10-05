using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyStateMachine : Node
{
    [Signal]
    public delegate void StateTransitionEventHandler(string prevState, string nextState);
    [Export] public Enemy enemy;
    [Export] EnemyState _initialState;
    private EnemyState _currState;
    private Dictionary<string, EnemyState> _allStates = new Dictionary<string, EnemyState>();
    

    public override void _Ready()
    {
        foreach (EnemyState state in GetChildren())
        {
            _allStates[state.Name] = state;
        }
        foreach(String state in _allStates.Keys) GD.Print(state);
        if (_initialState != null)
        {
            GD.Print(_initialState.Name);
            _currState = _allStates[_initialState.Name];
            _currState.Enter();
        }
        else
        {
            GD.PrintErr("EnemyStateMachine: No initial state set!");
        }
    }

    public override void _Process(double delta)
    {
        _currState.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currState.PhysicsUpdate(delta);
    }

    public void OnStateTransition(String oldState, String newState) // signal states will emit callback
    {
        _allStates[oldState].Exit();
        _currState = _allStates[newState];
        _currState.Enter();
    }
}