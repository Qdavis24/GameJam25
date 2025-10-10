using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class DeathState : EState
{
    [ExportCategory("special effects")] 
    [Export] private GpuParticles2D _deathEffect;
    private ParticleProcessMaterial _deathEffectMaterial;
    public override void Enter()
    {
        _deathEffectMaterial = (ParticleProcessMaterial) _deathEffect.ProcessMaterial;
        _stateMachine.Owner.Animations.Play("Die");
        Vector2 dir = (_stateMachine.InstanceContext.KnockBackDir).Normalized();
        Vector3 materialDir = new Vector3(dir.X, dir.Y, 0);
        _deathEffectMaterial.Direction = materialDir;
        _deathEffect.Emitting = true;
    }
    
    public override void Update(double delta)
    {
        if (!_stateMachine.Owner.Animations.IsPlaying())
            Owner.QueueFree();
    }

}