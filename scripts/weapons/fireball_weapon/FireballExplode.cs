using Godot;
using System;
using GameJam25.scripts.damage_system;

public partial class FireballExplode : Node2D
{
	[Export] private AudioStreamRandomizer _audioStreams;
	[Export] private float _lightIntensity;
	[Export] private GpuParticles2D _explodeEffect;
	[Export] private PointLight2D _explodeLight;
	[Export] public Hitbox _hitbox;
	

	private double _currTime;

	public override void _Ready()
	{
		Sfx.I.PlayFireballExplosion(_audioStreams, GlobalPosition, -15);
		_explodeLight.Energy = 0;
		_explodeEffect.Emitting = true;
		_explodeEffect.Finished += QueueFree;
	}

	public override void _PhysicsProcess(double delta)
	{
		_currTime = Math.Clamp(_currTime + delta, 0, .2);
		if (_currTime >= .2f) _hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
		_explodeLight.Energy = (float)Math.Sin(_currTime/.2 * Mathf.Pi) * _lightIntensity;
	}
	
	
}
