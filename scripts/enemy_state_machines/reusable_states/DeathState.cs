using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class DeathState : EState
{
	[ExportCategory("special effects")] [Export]
	private GpuParticles2D _deathEffect;

	private ParticleProcessMaterial _deathEffectMaterial;

	// Super's abstract methods below
	public override void Enter()
	{
		_stateMachine.Owner.Hurtbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
		_stateMachine.Owner.Hurtbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
		_deathEffectMaterial = (ParticleProcessMaterial)_deathEffect.ProcessMaterial;
		Vector2 dir = (_stateMachine.InstanceContext.KnockbackDir).Normalized();
		Vector3 materialDir = new Vector3(dir.X, dir.Y, 0);
		_deathEffectMaterial.Direction = materialDir;
		_deathEffect.Emitting = true;
	}

	public override void Exit()
	{
	}


	public override void Update(double delta)
	{
		if (!_deathEffect.Emitting)
			_stateMachine.Owner.QueueFree();
	}

	public override void PhysicsUpdate(double delta)
	{
	}
}
