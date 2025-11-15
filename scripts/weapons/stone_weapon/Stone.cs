using Godot;
using System;
using GameJam25.scripts.damage_system;

public partial class Stone : Node2D
{
	public Timer Timer;
	private Sprite2D _sprite;
	private GpuParticles2D _explosionParticles;
	private Hitbox _hitbox;

	public float Damage
	{
		get { return _hitbox.Damage; }
		set { _hitbox.Damage = value; }
	}

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_explosionParticles = GetNode<GpuParticles2D>("GPUParticles2D");
		_hitbox = GetNode<Hitbox>("Hitbox");
		Timer = GetNode<Timer>("Timer");

		Timer.Timeout += () =>
		{
			_sprite.Visible = true;
			_hitbox.SetDeferred(Area2D.PropertyName.Monitorable, true);
			_hitbox.SetDeferred(Area2D.PropertyName.Monitoring, true);
		};
		
		_hitbox.AreaEntered += OnAreaEntered;
	}
	
	
	public void OnAreaEntered(Area2D area)
	{
		if (!area.IsInGroup("EnemyHurtbox")) return;
		_explosionParticles.Emitting = true;
		_sprite.Visible = false;
		_hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
		_hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
		Timer.Start();
	}
}
