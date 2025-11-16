using System.Collections.Generic;
using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.custom_states;
public partial class ExplodeState : EState
{
    [ExportCategory("Explosion params")] [Export]
    private double _explosionDelay;

    [Export] private PackedScene _explosionAttackScene;
    private double _currTime;

    // Super's abstract methods below
    public override void Enter()
    {
        _currTime = GD.Randf() * _explosionDelay;
    }

    public override void Exit() {}
    public override void Update(double delta){}
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
            explosion.GlobalPosition = GlobalPosition;
            GetTree().Root.AddChild(explosion);
            _stateMachine.Owner.QueueFree();
        }
    }
}