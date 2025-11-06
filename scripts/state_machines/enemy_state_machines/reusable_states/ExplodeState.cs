using System.Collections.Generic;
using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class ExplodeState : EState
{
    [ExportCategory("Explosion params")] [Export]
    private double _explosionDelay;

    [Export] private PackedScene _explosionAttackScene;
    private double _currTime;

    public override void Enter()
    {
        _currTime = GD.Randf() * _explosionDelay;
    }

    public override void Exit()
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(GameManager.Instance.Player.GlobalPosition) >
            _stateMachine.Owner.AttackRange)
        {
            _stateMachine.TransitionTo("ChaseState");
        }

        _currTime -= delta;
        if (_currTime < 0)
        {
            var explosion = _explosionAttackScene.Instantiate<ExplodeAttack>();
            GetTree().Root.AddChild(explosion);
            explosion.GlobalPosition = GlobalPosition;
            Owner.QueueFree();
        }
    }
}