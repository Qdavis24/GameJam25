using System;
using GameJam25.scripts.damage_system;
using Godot;

namespace GameJam25.scripts.weapons.water_weapon;

public partial class Water : Node2D
{
	[ExportCategory("Sound FX")] 
	[Export] private AudioStream _wooshStream;
	[Export] private AudioStream _waterStream;
	
	[ExportCategory("Speed Behavior")] 
	[Export] private Curve _speedRamp;
	[Export] private float _timeToMaxSpeed = 2.0f;

	[ExportCategory("General Behavior")] 
	[Export] private float _loseControlDistance;
	
	[ExportCategory("Required Children")]
	[Export] public Timer Timer;
	[Export] public AnimatedSprite2D Sprite;
	[Export] private GpuParticles2D _explosionParticles;
	[Export] private GpuParticles2D _trailParticles;
	[Export] private Hitbox _hitbox;
	[Export] private Area2D _lockOnRange;

	
	public float Speed;
	
	private bool _active;
	private float _currTime;
	private Vector2 _launchDir;


	public float Damage
	{
		get { return _hitbox.Damage; }
		set { _hitbox.Damage = value; }
	}

	public override void _Ready()
	{
		_hitbox.Monitorable = false;
		_hitbox.Monitoring = false;
		_lockOnRange.Monitoring = false;
		_hitbox.AreaEntered += OnAreaEntered;
		_hitbox.BodyEntered += OnBodyEntered;
		_lockOnRange.BodyEntered += OnLockOnRangeEntered;
		Timer.Timeout += OnTimeout;
		
		Sfx.I.PlayFollowing(_waterStream, this, -30);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active) return;
		
		if (_launchDir != Vector2.Zero)
		{
			GlobalPosition += _launchDir * Speed * (float)delta;
			return;
		}

		var mouseDir = GetLocalMousePosition();
		if (mouseDir.LengthSquared() < Mathf.Pow(_loseControlDistance, 2)) _launchDir = mouseDir.Normalized();
		_currTime = Math.Clamp(_currTime + (float)delta, 0, _timeToMaxSpeed);
		var currSpeed = _speedRamp.Sample(_currTime / _timeToMaxSpeed) * Speed;
		Sprite.Rotation = mouseDir.Angle();
		GlobalPosition += mouseDir.Normalized() * currSpeed * (float)delta;
	}

	public void OnTimeout()
	{
		Sfx.I.Play2D(_wooshStream, GlobalPosition, -25);
		_trailParticles.Amount *= 2;
		Reparent(GameManager.Instance.World);
		_active = true;
		_hitbox.Monitorable = true;
		_hitbox.Monitoring = true;
		_lockOnRange.Monitoring = true;
	}

	private void Explode()
	{
		_hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
		_hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
		_lockOnRange.SetDeferred(Area2D.PropertyName.Monitoring, false);
		
		_explosionParticles.Emitting = true;
		Sprite.Visible = false;
		_explosionParticles.Finished += QueueFree;
		_active = false;
	}
	public void OnAreaEntered(Area2D area)
	{
		if (!_active) return;
		if (!area.IsInGroup("EnemyHurtbox")) return;
		Explode();
	}

	public void OnBodyEntered(Node2D body)
	{
		if (!_active) return;
		if (!body.IsInGroup("Obstacle")) return;
		Explode();
	}

	public void OnLockOnRangeEntered(Node2D body)
	{
		if (!body.IsInGroup("Enemies")) return;
		_launchDir = (body.GlobalPosition - GlobalPosition).Normalized();
		Sprite.Rotation = _launchDir.Angle();
	}
}
