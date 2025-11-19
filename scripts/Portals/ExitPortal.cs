using Godot;
using System;

public partial class ExitPortal : Node2D
{
	[Signal] public delegate void PlayerEnteredPortalEventHandler();
	[ExportCategory("Sound Fx")]
	[Export] private AudioStream _spawnNoise;
	[Export] private AudioStreamPlayer2D _portalSoundPlayer;
	
	[Export] private GpuParticles2D _particles;
	[Export] private PointLight2D _light;
	[Export] private Area2D _enterRange;
	
	[ExportGroup("Light Animation")]
	[Export] private float _maxTextureScale = 9f;
	[Export] private float _maxLightEnergy = 2.0f;
	[Export] private float _growDuration = 3.0f;
	[Export] private float _pulseSpeed = 3.0f;
	[Export] private float _pulseIntensityAmplitude = 1.5f;

	private float _pulseMinIntensity;
	private float _pulseMaxIntensity;
	
	private double _currTime;
	private bool _active;

	public override void _Ready()
	{
		_pulseMinIntensity = _light.Energy - _pulseIntensityAmplitude;
		_pulseMaxIntensity = _light.Energy + _pulseIntensityAmplitude;
		_portalSoundPlayer.Play();
		_particles.Emitting = true;
		_enterRange.BodyEntered += OnBodyEntered;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		

		if (!_active)
		{
			// Grow to full brightness with ease-in
			float progress = Mathf.Clamp((float)_currTime / _growDuration, 0f, 1f);
			_light.TextureScale = progress *  _maxTextureScale;
	
			if (progress >= 1.0f)
			{
				_active = true;
				_currTime = 0;
			}
		}
		else
		{
			
			// Intense pulsing effect
			float pulse = (Mathf.Sin((float)_currTime * _pulseSpeed + Mathf.Pi) + 1) / 2; // 0 to 1
			float intensity = Mathf.Lerp(_pulseMinIntensity, _pulseMaxIntensity, pulse);
			
			_light.Energy = intensity;
		}
		_currTime += delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!_active) return;
		if (!body.IsInGroup("Players")) return;
		Sfx.I.Play2D(_spawnNoise, GlobalPosition, -15);
		EmitSignal(SignalName.PlayerEnteredPortal);
	}
}
