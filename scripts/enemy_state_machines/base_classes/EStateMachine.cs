using System;
using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.enemy_state_machines.base_classes;

public partial class EStateMachine : Node2D
{
	[Export] public EState InitialState;
	private Dictionary<StateName, EState> _allStates = new();
	public EState CurrState;
	
	public Enemy Owner;
	public EInstanceContext InstanceContext;

	public bool IsActive;

	public override void _Ready()
	{
		Owner = (Enemy)GetParent();
		foreach (Node child in GetChildren())
		{
			if (child is EState)
			{
				EState state = (EState)child;
				_allStates[state.Name] = state;
			}
			if (child is EInstanceContext context)
			{
				InstanceContext = context;
			}
		}
	}
	public void Init()
	{
		if (InitialState != null)
		{
			CurrState = _allStates[InitialState.Name];
			CurrState.Enter();
		}
		else
		{
			GD.PrintErr("EnemyStateMachine: No initial state set!");
		}
	}
	public override void _Process(double delta)
	{
		if (!IsActive) return;
		CurrState.Update(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!IsActive) return;
		CurrState.PhysicsUpdate(delta);
	}
	public void TransitionTo(StateName newState)
	{
		CurrState.Exit();
		CurrState = _allStates[newState];
		CurrState.Enter();
	}
}
