using Godot;
using System;
using GameJam25.scripts.damage_system;

public partial class Stone : Node2D
{
	[ExportCategory("Sound Fx")] 
	[Export] private AudioStream _crumbleSound;
	
	[Export] private Hitbox _hitbox;
	
	[Export] public Timer Timer;
	private Sprite2D _sprite;
	

	public float Damage
	{
		set { _hitbox.Damage = value; }
	}

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");

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
		Sfx.I.Play2D(_crumbleSound, GlobalPosition, -25, GD.Randf() * 2);
		_sprite.Visible = false;
		_hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
		_hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
		Timer.Start();
	}
}
