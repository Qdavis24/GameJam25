using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class SpawnState : EState
{
	// Super's abstract methods below
	public override void Enter()
	{
		GD.Print("ENTER SPAWN");
		_stateMachine.Owner.Animations.Play("spawn");
		
		GD.Print(_stateMachine.Owner.Animations);
		
		_stateMachine.Owner.Animations.AnimationFinished += Switch; 
		
		// _stateMachine.TransitionTo("ChaseState");
		
		
		//_stateMachine.Owner.Animations.AnimationFinished += () => {
			//_stateMachine.TransitionTo("ChaseState");
		//};
	}
	
	private void Switch()
	{
		GD.Print("HELLLOOOO");
	}

	public override void Exit()
	{
		_stateMachine.Owner.Animations.Stop();
	}


	public override void Update(double delta)
	{
	}

	public override void PhysicsUpdate(double delta)
	{
	}
}
