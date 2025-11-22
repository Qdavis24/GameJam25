using Godot;
using System;
using GameJam25.scripts.enemy_state_machines;
using GameJam25.scripts.enemy_state_machines.base_classes;

public partial class MeeleState : EState
{
	[ExportCategory("Attack Behavior")] 
	[Export] private Timer _attackIntervalTimer;
	[Export] private PackedScene _meeleAttackScene;
	
	
	public override void _Ready()
	{
		base._Ready();
		_attackIntervalTimer.Timeout += OnTimeout;
	}

	public override void Enter()
	{
		_attackIntervalTimer.Start();
	}

	public override void Exit()
	{
		_attackIntervalTimer.Stop();
	}
	public override void Update(double delta){}
	public override void PhysicsUpdate(double delta)
	{
		if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(GameManager.Instance.Player.GlobalPosition) >
			Math.Pow(_stateMachine.Owner.AttackRange, 2))
		{
			_stateMachine.TransitionTo(StateName.ChaseState);
		}
	}

	public void OnTimeout()
	{
		var newAttack = CreateAttack();
		GetTree().Root.AddChild(newAttack);
	}
	
	// helper methods

	private Node2D CreateAttack()
	{
		var dirToPlayer = (GameManager.Instance.Player.GlobalPosition - _stateMachine.Owner.GlobalPosition).Normalized();
		var newAttack = _meeleAttackScene.Instantiate<Node2D>();
		newAttack.GlobalPosition = _stateMachine.Owner.GlobalPosition + dirToPlayer * (_stateMachine.Owner.AttackRange/2);
		newAttack.Rotation = dirToPlayer.Angle();
		return newAttack;
	}
	
}
