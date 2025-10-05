using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyStateMachine : Node
{
    [Signal]
    public delegate void StateTransitionEventHandler(string prevState, string nextState);
    [Export] public Enemy enemy;
    [Export] EnemyState initialState;
    private EnemyState currState;
    private Dictionary<string, EnemyState> allStates = new Dictionary<string, EnemyState>();

    public override void _Ready()
    {
        foreach (EnemyState state in GetChildren())
        {
            allStates[state.Name] = state;
        }
        foreach(String state in allStates.Keys) GD.Print(state);
        if (initialState != null)
        {
            GD.Print(initialState.Name);
            currState = allStates[initialState.Name];
            currState.Enter();
        }
        else
        {
            GD.PrintErr("EnemyStateMachine: No initial state set!");
        }
    }

    public override void _Process(double delta)
    {
        currState.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        currState.PhysicsUpdate(delta);
    }

    public void OnStateTransition(EnemyState oldState, EnemyState newState) // signal states will emit callback
    {
        oldState.Exit();
        currState = newState;
        newState.Enter();
    }
}