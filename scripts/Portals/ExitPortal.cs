using Godot;
using System;

public partial class ExitPortal : Node2D
{
    [Signal] public delegate void PlayerEnteredPortalEventHandler();
    
    [Export] private AudioStreamPlayer2D _portalSoundPlayer;
    
    [Export] private GpuParticles2D _particles;
    [Export] private PointLight2D _light;
    [Export] private Area2D _enterRange;
    
    [ExportGroup("Light Animation")]
    [Export] private float _maxLightEnergy = 2.0f;
    [Export] private float _growDuration = 3.0f;
    [Export] private float _pulseSpeed = 3.0f;
    [Export] private float _pulseMinIntensity = 0.5f;  // 50% to 100%
    [Export] private float _pulseMaxIntensity = 1.0f;
    
    private double _currTime;
    private bool _active;

    public override void _Ready()
    {
        _portalSoundPlayer.Play();
        _light.Energy = 0;
        _particles.Emitting = true;
        _enterRange.BodyEntered += OnBodyEntered;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        _currTime += delta;

        if (!_active)
        {
            // Grow to full brightness with ease-in
            float progress = Mathf.Clamp((float)_currTime / _growDuration, 0f, 1f);
            float easedProgress = Mathf.Pow(progress, 2); // Ease in quadratic
            _light.Energy = easedProgress * _maxLightEnergy;
    
            if (progress >= 1.0f)
            {
                _active = true;
                _currTime = 0;
            }
        }
        else
        {
            // Intense pulsing effect
            float pulse = (Mathf.Sin((float)_currTime * _pulseSpeed) + 1f) / 2f; // 0 to 1
            float intensity = Mathf.Lerp(_pulseMinIntensity, _pulseMaxIntensity, pulse);
            _light.Energy = _maxLightEnergy * intensity;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!_active) return;
        if (!body.IsInGroup("Players")) return;
        EmitSignal(SignalName.PlayerEnteredPortal);
    }
}