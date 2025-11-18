using Godot;
using System;

public partial class ExplodeAttack : Node2D
{
	[Export] private AudioStreamRandomizer _audioStreams;
	[Export] private GpuParticles2D _explodeEffect;
	[Export] private PointLight2D _explodeLight;
	[Export] private float _lightIntensity;

	private double _currTime;

	public override void _Ready()
	{
		Sfx.I.Play2D(_audioStreams, GlobalPosition, -10);
		_explodeLight.Energy = 0;
		_explodeEffect.Emitting = true;
		_explodeEffect.Finished += QueueFree;
	}

	public override void _PhysicsProcess(double delta)
	{
		_currTime = Math.Clamp(_currTime + delta, 0, .2);
		_explodeLight.Energy = (float)Math.Sin(_currTime/.2 * Mathf.Pi) * _lightIntensity;
	}
	
}
