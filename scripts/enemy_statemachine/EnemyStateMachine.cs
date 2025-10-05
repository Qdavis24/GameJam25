using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyStateMachine : Node
{
    [Export] EnemyState initialState;
    private EnemyState currState;
    private Dictionary<string, EnemyState> allStates = new Dictionary<string, EnemyState>();

    public override void _Ready()
    {
        foreach (EnemyState state in GetChildren())
        {
            allStates[state.Name] = state;
        }

        if (initialState != null)
        {
            currState = initialState;
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

    public void OnStateTransition(EnemyState oldState, EnemyState newState)
    {
        oldState.Exit();
        currState = newState;
        newState.Enter();
    }
}