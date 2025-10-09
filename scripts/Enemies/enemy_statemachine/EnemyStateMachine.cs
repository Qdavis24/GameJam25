using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyStateMachine : Node
{
    [Signal]
    public delegate void ChangeCurrentStateLabelEventHandler(string nextState);

    [Export] public Enemy enemy;
    public EnemyState CurrState;
    private Dictionary<string, EnemyState> _allStates = new Dictionary<string, EnemyState>();


    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is EnemyState)
            {
                EnemyState state = (EnemyState)child;
                state.StateMachine
                _allStates[state.Name] = state;
            }
        }

        foreach (String state in _allStates.Keys) ;
        if (_initialState != null)
        {
            
            EmitSignal(SignalName.ChangeCurrentStateLabel, _initialState.Name);
            CurrState = _allStates[_initialState.Name];
            CurrState.Enter();
        }
        else
        {
            GD.PrintErr("EnemyStateMachine: No initial state set!");
        }
    }

    public override void _Process(double delta)
    {
        CurrState.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrState.PhysicsUpdate(delta);
    }

    public void OnStateTransition(String prevState, String newState) // signal states will emit callback
    {
       
        EmitSignal(SignalName.ChangeCurrentStateLabel, newState);
        
        _allStates[prevState].Exit();
        CurrState = _allStates[newState];
        CurrState.Enter();
    }
}