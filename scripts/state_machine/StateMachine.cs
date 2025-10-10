using System;
using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.state_machine;
/*
 * StateMachine is the parent class for all subtype state machines
 * This class contains behaviour and data that all state machines need access to
 * 
 * 
 * a specific type is required for an Owner, and a Context hence subclasses defining explicit Owners and Contexts
 * are used (see EStateMachine)
 * 
 * StateMachine -> Owner (allows for access through state as states will have a reference to the state machine)
 * StateMachine <-> State (allows States to trigger state transition via TransitionTo)
 * StateMachine -> InstanceContext / GroupContext
 *
 * Use subtypes of this class by creating a Node and attaching the subtypes script
 * Once created, create nodes for each state as children of the state machine attaching their scripts
 *
 * StateMachine (subtype)
 * - State (subtype)
 * - State (subtype)
 * - State (subtype)
 *
 * ENSURE to make states children of state machine as requirement for initializing their references
 */
public partial class StateMachine : Node
{
    [Signal] public delegate void ChangeCurrentStateLabelEventHandler(string nextState);
    [Export] private State _initialState;
    private Dictionary<string, State> _allStates = new Dictionary<string, State>();
    public State CurrState;


    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is State)
            {
                State state = (State)child;
                _allStates[state.Name] = state;
            }
        }

        
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
    public void TransitionTo(String newState)
    {
        EmitSignal(SignalName.ChangeCurrentStateLabel, newState);
        CurrState.Exit();
        CurrState = _allStates[newState];
        CurrState.Enter();
    }
}