using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class SpawnState : EState
{
	[Export] private Timer _timer;
	// Super's abstract methods below
	public override void _Ready()
	{
		base._Ready();
		_timer.Timeout += () => _stateMachine.TransitionTo(StateName.ChaseState);
	}
	public override void Enter()
	{
		_timer.Start();
		_stateMachine.Owner.Animations.Play("spawn");
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
