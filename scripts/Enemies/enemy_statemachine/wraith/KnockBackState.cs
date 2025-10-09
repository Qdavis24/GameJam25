using Godot;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class KnockBackState : EnemyState
{
    private double _knockBackDuration = .5;
    private double _currTime;
    private float _baseSpeed = 700;

    public override void Enter()
    {
        _currTime = 0;
        GD.Print(Owner);
        Owner.animations.Play("TakeDamage");
        Owner.HitParticles.Emitting = true;
    }

    public override void Exit()
    {
        Owner.KnockBackDir = Vector2.Zero;
        Owner.HitParticles.Emitting = false;
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        _currTime += delta;
        float sample = Owner.KnockBackCurve.Sample((float)(_currTime / _knockBackDuration));
        float currSpeed = sample * _baseSpeed;
        Owner.Velocity = Owner.KnockBackDir.Normalized() * currSpeed;
        GD.Print(Owner.Velocity);
        if (_currTime >= _knockBackDuration)
        {
            EmitSignal(SignalName.StateTransition, Name, "ChaseState");
        }
    }
}