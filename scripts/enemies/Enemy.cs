using Godot;
using GameJam25.scripts.damage_system;
using GameJam25.scripts.enemies;
using GameJam25.scripts.enemy_state_machines.base_classes;

public partial class Enemy : CharacterBody2D
{
	[Export] public EnemyType Type;
	
	[ExportCategory("Sound Fx")] 
	[Export] private AudioStreamRandomizer _hitSounds;
	
	[ExportCategory("stats")] 
	[Export] private float _maxHealth; //starting health
	[Export] public int XpReward = 3;

	[ExportCategory("Distance Ranges")] 
	[Export] public Area2D SteeringRange;
	[Export] public Hurtbox Hurtbox;
	[Export] public int AttackRange;
	

	[ExportCategory("References")] 
	[Export] public AnimatedSprite2D Animations;
	[Export] private Timer _damageIntervalTimer;
	[Export] private EStateMachine _stateMachine;
	[Export] private ShaderMaterial _flashShader;
	[Export] private GpuParticles2D _trailParticles;

	private uint _originalCollisionLayer;
	private uint _originalCollisionMask;

	public bool CanTakeDamage = true;
	public bool InPool = true;
	
	public float Health; // public so states can use this



	public void Enable(Vector2 spawnPosition)
	{
		InPool = false;
		_trailParticles.ProcessMode = ProcessModeEnum.Inherit; // maybe dont if bad performance
		_trailParticles.Emitting = true;
		GlobalPosition = spawnPosition;
		Health = _maxHealth;
    
		// Collision and monitoring
		CanTakeDamage = true;
		CollisionLayer = _originalCollisionLayer;
		CollisionMask = _originalCollisionMask;
		SteeringRange.Monitorable = true;
		SteeringRange.Monitoring = true;
		Hurtbox.Monitorable = true;
		Hurtbox.Monitoring = true;
    
		// Animations and visibility
		Visible = true;
		Animations.Play(); // Or "idle"
		
		_stateMachine.IsActive = true;
		_stateMachine.Init();
	}

	public void Disable()
	{
		InPool = true;
		_trailParticles.ProcessMode = ProcessModeEnum.Inherit; // maybe dont if bad performance
		_trailParticles.Emitting = true;
		// Collision and monitoring
		CanTakeDamage = false;
		CollisionLayer = 0;
		CollisionMask = 0;
		SteeringRange.Monitorable = false;
		SteeringRange.Monitoring = false;
		Hurtbox.Monitorable = false;
		Hurtbox.Monitoring = false;
    
		// Animations and visibility
		Visible = false;
		Animations.Stop();
    
		// Stop timers
		_damageIntervalTimer.Stop();
		Animations.Material = null;

		_stateMachine.IsActive = false;
	}
	
	public void TakeDamage(float amount, float knockbackWeight, Vector2 direction)
	{
		if (_stateMachine.CurrState.Name == "DeathState") return;
		Health -= amount;
		_stateMachine.InstanceContext.KnockbackDir = direction.Normalized();
		_stateMachine.InstanceContext.KnockbackWeight = knockbackWeight;
		_stateMachine.TransitionTo("KnockbackState");
	}

	public override void _Ready()
	{
		// Cache collision settings from editor
		_originalCollisionLayer = CollisionLayer;
		_originalCollisionMask = CollisionMask;
		
		double randomScale = GD.RandRange(1, 1.12);
		Scale *= (float) randomScale;
		Hurtbox.AreaEntered += OnEnemyHurtBoxEntered;
		_damageIntervalTimer.Timeout += () =>
		{
			CanTakeDamage = true;
			Animations.Material = null;
		};
		
		Health = _maxHealth;
	}


	private void OnEnemyHurtBoxEntered(Area2D area)
	{
		if (!CanTakeDamage || !area.IsInGroup("PlayerHitbox")) return;
		CanTakeDamage = false;
		Hitbox hb = (Hitbox)area;
		Animations.Material = _flashShader;
		_damageIntervalTimer.Start();
		Sfx.I.Play2D(_hitSounds, GlobalPosition, -20);
		TakeDamage(hb.Damage, hb.KnockbackWeight, (GlobalPosition - hb.GlobalPosition));
	}
}
