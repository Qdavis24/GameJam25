using Godot;
using System;

public partial class ExplodeAttack : Node2D
{
    [Signal]
    public delegate void AttackFinishedEventHandler();

    [Export] private AudioStreamPlayer2D _audioStreamPlayer;
    [Export] private AudioStream[] _audioStreams;
    [Export] private GpuParticles2D _explodeEffect;
    [Export] private PointLight2D _explodeLight;
    [Export] private float _lightIntensity;

    private double _currTime;

    public override void _Ready()
    {
        _audioStreamPlayer.Stream = _audioStreams[GD.Randi()%_audioStreams.Length];
        _audioStreamPlayer.Play();
        _explodeLight.Energy = 0;
        _explodeEffect.Emitting = true;
        _explodeEffect.Finished += OnFinished;
    }

    public override void _PhysicsProcess(double delta)
    {
        _currTime = Math.Clamp(_currTime + delta, 0, .2);
        _explodeLight.Energy = (float)Math.Sin(_currTime/.2 * Mathf.Pi) * _lightIntensity;
    }

    private void OnFinished()
    {
        QueueFree();
    }
}