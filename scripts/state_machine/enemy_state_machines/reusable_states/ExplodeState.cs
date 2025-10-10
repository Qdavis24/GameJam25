using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class ExplodeState : EState
{
    [ExportCategory("Explosion params")]
    [Export] private double _explosionDelay;
    [Export] private GpuParticles2D _explosionEffect;
    
    private double _currTime;
    private bool _isExploding;

    public override void Enter()
    {
        _currTime = GD.Randf() * _explosionDelay;
        _stateMachine.Owner.Animations.Play("Attack");
        _isExploding = false;
    }

    public override void Exit()
    {
    }

    public override void Update(double delta)
    {
        _currTime -= delta;
        if (_currTime < 0 && !_isExploding)
        {
            _explosionEffect.Emitting = true;
            _isExploding = true;
            _stateMachine.Owner.Animations.Play("Die");
        }
        else if (_currTime < 0 & _isExploding && _explosionEffect.Emitting == false)
        {
            _stateMachine.Owner.QueueFree();
        }
        
        
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(_stateMachine.InstanceContext.CurrentTarget
                .GlobalPosition) > _stateMachine.Owner.AttackRange)
        {
            _stateMachine.TransitionTo("ChaseState");
        }
    }
}