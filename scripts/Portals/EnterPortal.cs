using Godot;
using System;

public partial class EnterPortal : Node2D
{
    [Signal] public delegate void PortalOpenEventHandler();
    
    [Export] private AudioStreamPlayer2D _portalSoundPlayer;
    [Export] private GpuParticles2D _particles;
    [Export] private PointLight2D _light;
    [Export] private GpuParticles2D _poofEffect;
    
    [ExportGroup("Light Animation")]
    [Export] private float _maxTextureScale = 9f;
    [Export] private float _growDuration = 3.0f;
    [Export] private float _portalOpenDelay = 1.0f;
    [Export] private float _lightFadeOutDuration = 1.5f;  // How long light fades

    [ExportGroup("Audio")] 
    [Export] private AudioStream _spawnNoise;
    [Export] private float _audioFadeOutDuration = 1.0f;
    
    private double _currTime;
    private bool _hasEmittedSignal;
    private bool _isFadingOut;
    private float _fadeOutTimer;
    private float _initialVolume;

    public override void _Ready()
    {
        _portalSoundPlayer.Play();
        _initialVolume = _portalSoundPlayer.VolumeDb;
        _particles.Emitting = true;
        _particles.Finished += OnParticlesFinished;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        _currTime += delta;
        
        if (!_isFadingOut)
        {
            // Grow light effect
            float progress = Mathf.Clamp((float)_currTime / _growDuration, 0f, 1f);
            _light.TextureScale = _maxTextureScale * progress;
            
            // Emit signal after particles fully open + delay
            if (!_hasEmittedSignal && progress >= 1.0f && _currTime >= _growDuration + _portalOpenDelay)
            {
                _hasEmittedSignal = true;
                _poofEffect.Emitting = true;
                _light.Energy *= 2;
                _particles.Visible = false;
                Sfx.I.Play2D(_spawnNoise, GlobalPosition,-15);
                EmitSignal(SignalName.PortalOpen);
                StartFadeOut();
            }
        }
        else
        {
            // Fade out both audio and light
            _fadeOutTimer += (float)delta;
            float fadeProgress = Mathf.Clamp(_fadeOutTimer / _lightFadeOutDuration, 0f, 1f);
            
            // Fade light
            _light.Energy = Mathf.Lerp(_light.Energy, 0f, fadeProgress);
            
            // Fade audio (faster if audio fade is shorter)
            float audioFadeProgress = Mathf.Clamp(_fadeOutTimer / _audioFadeOutDuration, 0f, 1f);
            _portalSoundPlayer.VolumeDb = Mathf.Lerp(_initialVolume, -80f, audioFadeProgress);
            
            if (audioFadeProgress >= 1.0f)
            {
                _portalSoundPlayer.Stop();
            }
        }
    }
    
    private void StartFadeOut()
    {
        _isFadingOut = true;
        _fadeOutTimer = 0f;
    }
    
    private void OnParticlesFinished()
    {
        QueueFree();
    }
}