using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class KnockbackState : EState
{
    private double _knockBackDuration = .5;
    private double _currTime;
    private float _baseSpeed = 700;

    public override void Enter()
    {
        _currTime = 0;
        _stateMachine.Owner.Animations.Play("TakeDamage");
        _stateMachine.Owner.HitParticles.Emitting = true;
    }

    public override void Exit()
    {
        _stateMachine.InstanceContext.KnockBackDir = Vector2.Zero;
        _stateMachine.Owner.HitParticles.Emitting = false;
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        _currTime += delta;
        float sample = _stateMachine.Owner.KnockbackCurve.Sample((float)(_currTime / _knockBackDuration));
        float currSpeed = sample * _baseSpeed;
        _stateMachine.Owner.Velocity = _stateMachine.InstanceContext.KnockBackDir.Normalized() * currSpeed;
        _stateMachine.Owner.MoveAndSlide();
        if (_currTime >= _knockBackDuration)
        {
            _stateMachine.TransitionTo("IdleState");
        }
    }
}