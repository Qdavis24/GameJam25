using GameJam25.scripts.sfx_system;
using Godot;

public partial class Sfx : Node
{
    public static Sfx I;

    private const int PoolSize = 32;
    private PooledAudioStreamPlayer2D[] _audioPool2D;
    private PooledAudioStreamPlayer[] _audioPool;
    private bool[] _pool2DInUse;
    private bool[] _poolInUse;
    

    private bool _footstepPlaying = false;
    private static bool _explosionCooldown = false;
    private const float ExplosionCooldownTime = 0.12f;

    private const string SfxBus = "SFX";
    private const string MusicBus = "Music";
    private const string ExplosionBus = "Explosion";

    public override void _EnterTree() => I = this;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        InitializePool();
    }

    private void InitializePool()
    {
        _audioPool2D = new PooledAudioStreamPlayer2D[PoolSize];
        _pool2DInUse = new bool[PoolSize];
        
        _audioPool = new PooledAudioStreamPlayer[PoolSize];
        _poolInUse = new bool[PoolSize];

        for (int i = 0; i < PoolSize; i++)
        {
            var p2D = new PooledAudioStreamPlayer2D
                { Bus = SfxBus, OnFinishedCallback = OnPlayerFinished, PoolIndex = i };
            AddChild(p2D);
            _audioPool2D[i] = p2D;
            
            var p = new PooledAudioStreamPlayer
                { Bus = SfxBus, OnFinishedCallback = OnPlayerFinished, PoolIndex = i };
            _audioPool[i] = p;
            AddChild(p);


        }
    }

    public void OnPlayerFinished(int i, AudioPlayerType type)
    {
        switch (type)
        {
            case AudioPlayerType.Regular:
                _poolInUse[i] = false;
                break;
            case AudioPlayerType.Positional:
                _pool2DInUse[i] = false;
                _audioPool2D[i].Reparent(this);
                break;
        }
        
    }

    private T GetAvailablePlayer<T>(AudioPlayerType type)
    {
        switch (type)
        {
            case AudioPlayerType.Regular:
                for (int i = 0; i < PoolSize; i++)
                {
                    if (!_poolInUse[i])
                    {
                        _poolInUse[i] = true;
                        return (T)(object)_audioPool[i];
                    }
                }
                break;
            case AudioPlayerType.Positional:
                for (int i = 0; i < PoolSize; i++)
                {
                    if (!_pool2DInUse[i])
                    {
                        _pool2DInUse[i] = true;
                        return (T)(object)_audioPool2D[i];
                    }
                }
                break;
        }
        

        return (T)(object)null; // Pool exhausted
    }

    public void PlayFootstep(AudioStream stream)
    {
        if (_footstepPlaying || stream == null) return;

        var p = GetAvailablePlayer<AudioStreamPlayer>(AudioPlayerType.Regular);
        if (p == null) return;

        _footstepPlaying = true;

        void OnFootstepFinished()
        {
            _footstepPlaying = false;
            p.Finished -= OnFootstepFinished;
        }
        p.Stream = stream;
        p.VolumeDb = -10;
        p.PitchScale = (float)GD.RandRange(0.9, 1.1);
        p.Bus = SfxBus;
        
        p.Play();
    }

    public void Play2D(AudioStream stream, Vector2 pos, float volumeDb = -6f, float pitch = 1f)
    {
        if (stream == null)
        {
            GD.PrintErr("SFX stream is null");
            return;
        }

        var p = GetAvailablePlayer<AudioStreamPlayer2D>(AudioPlayerType.Positional);
        if (p == null) return;

        p.Stream = stream;
        p.GlobalPosition = pos;
        p.VolumeDb = volumeDb;
        p.PitchScale = pitch;
        
        p.Play();
    }

    public AudioStreamPlayer2D PlayFollowing(AudioStream stream, Node2D target, float volumeDb = -6f, float pitch = 1f)
    {
        if (stream == null || target == null) return null;
        
        var p = GetAvailablePlayer<AudioStreamPlayer2D>(AudioPlayerType.Positional);
        if (p == null) return p;
        p.Stream = stream;
        p.VolumeDb = volumeDb;
        p.PitchScale = pitch;
        p.Bus = SfxBus;
        
        p.Reparent(target);
        p.Play();

        return p;
    }

    public void PlayUi(AudioStream stream, float volumeDb = -6f, float pitch = 1f)
    {
        if (stream == null)
        {
            GD.PrintErr("Sfx::PlayUI : stream = null");
            return;
        }

        var p = GetAvailablePlayer<AudioStreamPlayer>(AudioPlayerType.Regular);
        if (p == null) return;
        p.Stream = stream;
        p.VolumeDb = volumeDb;
        p.PitchScale = pitch;
        p.Bus = SfxBus;
        
        p.Play();
    }

    public void PlayFireballExplosion(AudioStream stream, Vector2 pos, float volumeDb = -6f)
    {
        if (stream == null || _explosionCooldown) return;

        _explosionCooldown = true;

        var p = GetAvailablePlayer<AudioStreamPlayer2D>(AudioPlayerType.Positional);
        if (p == null) return;
        p.Stream = stream;
        p.GlobalPosition = pos;
        p.VolumeDb = volumeDb;
        p.Bus = ExplosionBus;
        p.Play();

        var timer = GetTree().CreateTimer(ExplosionCooldownTime);
        timer.Timeout += ResetExplosionCooldown;
    }

    // Signal Callbacks
    private void ResetExplosionCooldown()
    {
        _explosionCooldown = false;
    }
}