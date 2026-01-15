using GameJam25.scripts.enemy_state_machines.base_classes;
using Godot;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class DeathState : EState
{
	[ExportCategory("special effects")] 
	[Export] private GpuParticles2D _deathEffect;
	[Export] private PackedScene _healthScene;

	[ExportCategory("PickupDrops")] 
	[Export] private float _healthDropChance;
	[Export] private float _xpDropChance;
	
	
	private ParticleProcessMaterial _deathEffectMaterial;

	// Super's abstract methods below
	public override void _Ready()
	{
		base._Ready();
		_deathEffect.Finished += Exit;
	}
	public override void Enter()
	{
		_stateMachine.Owner.CollisionLayer = 0;
		_stateMachine.Owner.Animations.Play("death");
		_stateMachine.Owner.CanTakeDamage = false;
		GameManager.Instance.EnemyDeathUpdateKillCounter();
		_deathEffectMaterial = (ParticleProcessMaterial)_deathEffect.ProcessMaterial;
		Vector2 dir = (_stateMachine.InstanceContext.KnockbackDir).Normalized();
		Vector3 materialDir = new Vector3(dir.X, dir.Y, 0);
		_deathEffectMaterial.Direction = materialDir;
		_deathEffect.Emitting = true;
	}

	public override void Exit()
	{
		_stateMachine.Owner.Animations.Stop();
		GameManager.Instance.PickupPool.SpawnPickupAt(PickupType.Xp, GlobalPosition, _stateMachine.Owner.XpReward);
		if (GD.RandRange(0, 1) < _healthDropChance)
		{
			GameManager.Instance.PickupPool.SpawnPickupAt(PickupType.Health, GlobalPosition);
		}
		GameManager.Instance.EnemyPool.ReturnEnemy(_stateMachine.Owner);
	}


	public override void Update(double delta)
	{
	}

	public override void PhysicsUpdate(double delta)
	{
	}
}
