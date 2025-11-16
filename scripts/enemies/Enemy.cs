using Godot;
using GameJam25.scripts.damage_system;
using GameJam25.scripts.enemy_state_machines.base_classes;

public partial class Enemy : CharacterBody2D
{
	[ExportCategory("Sound Fx")] 
	[Export] private AudioStreamRandomizer _hitSounds;
	
	[ExportCategory("stats")] 
	[Export] private float _maxHealth; //starting health

	[ExportCategory("Distance Ranges")] 
	[Export] public Area2D SteeringRange;
	[Export] public int AttackRange;

	[ExportCategory("Miscellaneous")] 
	[Export] public AnimatedSprite2D Animations;
	[Export] private Timer _flashTimer;
	[Export] private EStateMachine _stateMachine;
	[Export] private ShaderMaterial _flashShader;
	[Export] private Timer _damageIntervalTimer;
	
	
	public Area2D Hurtbox;

	public float Health; // public so states can use this
	
	private bool _canTakeDamage = true;


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
		Hurtbox = GetNode<Area2D>("Hurtbox");
		
		Hurtbox.AreaEntered += OnEnemyHurtBoxEntered;
		_flashTimer.Timeout += () => Animations.Material = null;
		_damageIntervalTimer.Timeout += () => _canTakeDamage = true;
		
		Health = _maxHealth;
	}


	private void OnEnemyHurtBoxEntered(Area2D area)
	{
		if (!_canTakeDamage) return;
		if (!area.IsInGroup("PlayerHitbox")) return;
		_canTakeDamage = false;
		_damageIntervalTimer.Start();
		Hitbox hb = (Hitbox)area;
		Animations.Material = _flashShader;
		_flashTimer.Start();
		Sfx.I.Play2D(_hitSounds, GlobalPosition, -40);
		


		TakeDamage(hb.Damage, hb.KnockbackWeight, (GlobalPosition - hb.GlobalPosition));
	}
}
