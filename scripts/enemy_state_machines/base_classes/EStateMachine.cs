using System;
using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.enemy_state_machines.base_classes;

public partial class EStateMachine : Node2D
{
	[Signal] public delegate void ChangeCurrentStateLabelEventHandler(string nextState);
	[Export] private EState _initialState;
	private Dictionary<string, EState> _allStates = new();
	public EState CurrState;
	
	public Enemy Owner;
	public EInstanceContext InstanceContext;


	public override void _Ready()
	{
		Owner = (Enemy)GetParent();
		Owner.Animations.Play("spawn");
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
