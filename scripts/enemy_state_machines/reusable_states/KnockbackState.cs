using System;
using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class KnockbackState : EState
{
    [ExportCategory("Knockback")] [Export] public Curve KnockbackCurve;

    [Export] private double _knockBackDuration;
    private float _knockbackWeight;

    [ExportCategory("special effects")] [Export]
    private GpuParticles2D _hitEffect;

    private ParticleProcessMaterial _hitEffectMaterial;

    private double _currTime;

    // Super's abstract methods below
    public override void Enter()
    {
        _knockbackWeight = _stateMachine.InstanceContext.KnockbackWeight;
        _hitEffectMaterial = (ParticleProcessMaterial)_hitEffect.ProcessMaterial;
        _currTime = 0;
        Vector2 dir = (_stateMachine.InstanceContext.KnockbackDir).Normalized();
        Vector3 materialDir = new Vector3(dir.X, dir.Y, 0);
        _hitEffectMaterial.Direction = materialDir;
        _hitEffect.Emitting = true;
    }

    public override void Exit()
    {
        _hitEffect.Emitting = false;
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        _currTime += delta;
        float sample = KnockbackCurve.Sample((float)(_currTime / _knockBackDuration));
        float currSpeed = sample * _knockbackWeight;
        _stateMachine.Owner.Velocity = _stateMachine.InstanceContext.KnockbackDir.Normalized() * currSpeed;
        _stateMachine.Owner.MoveAndSlide();
        if (_currTime >= _knockBackDuration)
        {
            if (_stateMachine.Owner.Health <= 0) _stateMachine.TransitionTo("DeathState");
            else _stateMachine.TransitionTo("ChaseState");
        }
    }
    
}