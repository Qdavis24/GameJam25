using GameJam25.scripts.damage_system;
using Godot;


public partial class Fireball : Node2D
{
	[ExportCategory("sound FX")] 
	
	[Export] private AudioStream _fireStream;
	[Export] private float _fireVolume;
	
	
	[Export] private PackedScene _explosionParticles;
	[Export] private Hitbox _hitbox;

	public float Damage
	{
		get
		{
			return _hitbox.Damage;
		}
		set
		{
			_hitbox.Damage = value;
		}
	}

	public float ExplosionScale;

	public override void _Ready()
	{
		_hitbox.AreaEntered += OnAreaEntered;
		_hitbox.BodyEntered += OnBodyEntered;
		Sfx.I.PlayFollowing(_fireStream,  this,_fireVolume,.5f + GD.Randf());
	}

	private void SpawnExplosion()
	{
		var explosion = _explosionParticles.Instantiate<FireballExplode>();
		explosion._hitbox.Damage = Damage;
		explosion.Scale *= ExplosionScale;
		GetTree().Root.CallDeferred("add_child",  explosion);
		explosion.GlobalPosition = GlobalPosition;
	}
	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("EnemyHurtbox"))
		{
			SpawnExplosion();
			QueueFree();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Obstacle"))
		{
			SpawnExplosion();
			QueueFree();
		}
	}

}
