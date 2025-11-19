using GameJam25.scripts.enemies.owl_wraith_package;
using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.custom_states;

public partial class ShootState : EState
{
	[Export] private Timer _timer;
	[Export] private PackedScene _projectile;
	[Export] private int _projectileSpeed;

	public override void _Ready()
	{
		base._Ready();
		_timer.Timeout += OnTimeout;
	}
	public override void Enter()
	{
		_timer.Start();
	}

	public override void Exit()
	{
		_timer.Stop();
	}
	public override void Update(double delta){}

	public override void PhysicsUpdate(double delta)
	{
		if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(GameManager.Instance.Player.GlobalPosition) >
			_stateMachine.Owner.AttackRange * _stateMachine.Owner.AttackRange) // transition to chase
		{
			_stateMachine.TransitionTo("ChaseState");
		}
		
	}

	private void OnTimeout()
	{
		var newShadowBall = _projectile.Instantiate<ShadowBall>();
		newShadowBall.Init((GameManager.Instance.Player.GlobalPosition -_stateMachine.Owner.GlobalPosition).Normalized(), _projectileSpeed);
		newShadowBall.GlobalPosition = _stateMachine.Owner.GlobalPosition;
		GetTree().Root.AddChild(newShadowBall);
	}
	
}
