using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class SpawnState : EState
{
	// Super's abstract methods below
	public override void Enter()
	{
		_stateMachine.Owner.Animations.AnimationFinished += Switch; 
	}
	
	private void Switch()
	{
		_stateMachine.TransitionTo("ChaseState");
	}

	public override void Exit()
	{
	}


	public override void Update(double delta)
	{
	}

	public override void PhysicsUpdate(double delta)
	{
	}
}
