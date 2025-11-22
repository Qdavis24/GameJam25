using Godot;

public partial class Sfx : Node
{
    public static Sfx I;
    
    private const int PoolSize = 32;
    private AudioStreamPlayer2D[] _audioPool;
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
        ProcessMode = Node.ProcessModeEnum.Always;
        InitializePool();
    }
    
    private void InitializePool()
    {
        _audioPool = new AudioStreamPlayer2D[PoolSize];
        _poolInUse = new bool[PoolSize];
        
        for (int i = 0; i < PoolSize; i++)
        {
            var p = new AudioStreamPlayer2D { Bus = SfxBus };
            AddChild(p);
            _audioPool[i] = p;
            
            int index = i; // Capture for lambda
            p.Finished += () => _poolInUse[index] = false;
        }
    }
    
    private AudioStreamPlayer2D GetAvailablePlayer()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            if (!_poolInUse[i])
            {
                _poolInUse[i] = true;
                return _audioPool[i];
            }
        }
        return null; // Pool exhausted
    }

    public void PlayFootstep(AudioStream stream)
    {
        if (_footstepPlaying || stream == null) return;
    
        var p = GetAvailablePlayer();
        if (p == null) return;
    
        p.Stream = stream;
        p.VolumeDb = -6f;
        p.PitchScale = (float)GD.RandRange(0.9, 1.1);
    
        _footstepPlaying = true;
    
        void OnFootstepFinished()
        {
            _footstepPlaying = false;
            p.Finished -= OnFootstepFinished;
        }
    
        p.Finished += OnFootstepFinished;
        p.Play();
    }
    
    public void Play2D(AudioStream stream, Vector2 pos, float volumeDb = -6f, float pitch = 1f)
    {
        if (stream == null)
        {
            GD.PrintErr("SFX stream is null");
            return;
        }

        var p = GetAvailablePlayer();
        if (p == null) return;

        p.Stream = stream;
        p.GlobalPosition = pos;
        p.VolumeDb = volumeDb;
        p.PitchScale = pitch;
        p.Play();
    }
    
    public void PlayFollowing(AudioStream stream, Node2D target, float volumeDb = -6f, float pitch = 1f)
    {
        if (stream == null || target == null) return;

        // PlayFollowing needs a different approach since it parents to the target
        // We can't use the pool for this one since pool players are parented to Sfx
        var p = new AudioStreamPlayer2D
        {
            Stream = stream,
            VolumeDb = volumeDb,
            PitchScale = pitch,
            Bus = SfxBus
        };
    
        target.AddChild(p);
        p.Finished += () => p.QueueFree();
        p.Play();
    }
    
    public void PlayUi(AudioStream stream, float volumeDb = -6f, float pitch = 1f)
    {
        if (stream == null) return;

        var p = new AudioStreamPlayer
        {
            Stream = stream,
            VolumeDb = volumeDb,
            PitchScale = pitch,
            Bus = SfxBus
        };

        AddChild(p);
        p.Finished += () => p.QueueFree();
        p.Play();
    }
    
    public void PlayFireballExplosion(AudioStream stream, Vector2 pos, float volumeDb = -6f)
    {
        if (stream == null || _explosionCooldown) return;

        _explosionCooldown = true;

        var p = new AudioStreamPlayer
        {
            Stream = stream,
            VolumeDb = volumeDb,
            Bus = ExplosionBus
        };

        AddChild(p);
        p.Finished += () => p.QueueFree();
        p.Play();

        var timer = GetTree().CreateTimer(ExplosionCooldownTime);
        timer.Timeout += () => _explosionCooldown = false;
    }
}