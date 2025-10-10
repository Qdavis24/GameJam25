using System;
using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class KnockbackState : EState
{
    [ExportCategory("Knockback")] 
    [Export] public Curve KnockbackCurve;
    [Export] private int KnockbackWeight;
    [Export] private double _knockBackDuration;
    
    [ExportCategory("special effects")] 
    [Export] private GpuParticles2D _hitEffect;
    private ParticleProcessMaterial _hitEffectMaterial;
    
    private double _currTime;
    
    public override void Enter()
    {
        _hitEffectMaterial = (ParticleProcessMaterial) _hitEffect.ProcessMaterial;
        _currTime = 0;
        _stateMachine.Owner.Animations.Play("TakeDamage");
        Vector2 dir = (_stateMachine.InstanceContext.KnockBackDir).Normalized();
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
        float currSpeed = sample * KnockbackWeight;
        _stateMachine.Owner.Velocity = _stateMachine.InstanceContext.KnockBackDir.Normalized() * currSpeed;
        _stateMachine.Owner.MoveAndSlide();
        if (_currTime >= _knockBackDuration)
        {
            _stateMachine.TransitionTo("IdleState");
        }
    }
}