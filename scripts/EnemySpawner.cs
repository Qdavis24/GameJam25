using System;
using Godot;
using System.Collections.Generic;
using GameJam25.scripts.damage_system;
using GameJam25.scripts.enemies;

public partial class EnemySpawner : Node2D
{
    [Signal]
    public delegate void SpawnerDestroyedEventHandler(Vector2 pos);

    [ExportCategory("Spawner Boss")]
    [Export] private Hurtbox _bossHurtbox;
    [Export] private ShaderMaterial _flashShaderMaterial;
    [Export] private Timer _flashTimer;

    [ExportCategory("Spawning Light")] 
    [Export] private PointLight2D _light;
    [Export] private Curve _lightRamp;
    [Export] private float _maxEnergy;

    [ExportCategory("Spawner Config")] 
    [Export] private Timer _timer;
    [Export] private Vector2 _spawnRadius;
    [Export] private int _numEnemiesPerWave = 1;
    [Export] private int _numWaves;

    [ExportCategory("Ally")] 
    [Export] private PackedScene _allyPackedScene;

    [ExportCategory("Bosses")] 
    [Export] private AnimatedSprite2D _raccoonBoss;
    [Export] private AnimatedSprite2D _ratBoss;
    [Export] private AnimatedSprite2D _bunnyBoss;
    
    [ExportCategory("Sounds")]
    [Export] private AudioStream _bossDeathSounds;
    [Export] private AudioStream _bossHitSounds;
    [Export] private AudioStream _bossExplosionSounds;

    [ExportCategory("Destructor Config")] [Export]
    private Node2D _destructorBody;

    [Export] private GpuParticles2D _explosionParticles;
    [Export] private GpuParticles2D _oozeParticles;
    [Export] private float _explosionLightMaxIntensity;

    // have to manually set these cause godot won't export an array of enum values :(
    private EnemyType[] _enemyTypes = {
        EnemyType.OwlWraith, EnemyType.BunnyWraith, EnemyType.DeerWraith
    };

    private float[] _enemyTypeSpawnChance = {.2f, .4f, .4f};

    
    private Dictionary<string, AnimatedSprite2D> _bosses;

    private Ally _ally;

    private float _crystalHealth;
    private double _currTime;
    private int _currWave;

    private AnimatedSprite2D _currBossSprite;

    private bool _active = true;

    public void Init(float maxCrystalHealth, float interval, float enemiesPerWave, float numWaves)
    {
        _crystalHealth = maxCrystalHealth;
        _numEnemiesPerWave = (int)enemiesPerWave;
        _numWaves = (int)numWaves;
        _timer.WaitTime = interval;
    }

    public override void _Ready()
    {
        _bosses = new Dictionary<string, AnimatedSprite2D>()
        {
            { "raccoon", _raccoonBoss },
            { "bunny", _bunnyBoss },
            { "rat", _ratBoss }
        };
        string boss = GameManager.Instance.Bosses[0];
        GameManager.Instance.Bosses.RemoveAt(0);
        _bosses[boss].Visible = true;
        _currBossSprite = _bosses[boss];
        
        SpawnAlly();
        OnTimeout();
        _currWave = 0;
        _light.Energy = 0;
        _bossHurtbox.AreaEntered += OnBossHurtboxEntered;
        _timer.Timeout += OnTimeout;
        _flashTimer.Timeout += () => _currBossSprite.Material = null;
    }

    public override void _Process(double delta)
    {
        _currTime += delta;
        var energySample = _lightRamp.Sample((float)(_currTime / _timer.WaitTime));
        _light.Energy = energySample * _maxEnergy;
    }

    private void OnTimeout()
    {
        _currTime = 0;
        if (_currWave >= _numWaves) return;
        _currWave++;
        float cumProb = 0;
        for (int i = 0; i < _enemyTypes.Length; i++)
        {
            cumProb += _enemyTypeSpawnChance[i];
            if (GD.Randf() < cumProb)
            {
                SpawnGroup(_enemyTypes[i], _numEnemiesPerWave);
                return;
            }
        }
    }

    private void SpawnGroup(EnemyType enemyType, int numEnemies)
    {
        for (int i = 0; i < numEnemies; i++)
        {
            var globalPosition = GlobalPosition + new Vector2(
                (_spawnRadius.X * GD.Randf() + 150) * (GD.Randf() < 0.5f ? -1 : 1),
                (_spawnRadius.Y * GD.Randf() + 150) * (GD.Randf() < 0.5f ? -1 : 1)
            );
            GameManager.Instance.EnemyPool.SpawnEnemyAt(enemyType, globalPosition);
        }
    }
    
    private void OnBossHurtboxEntered(Area2D area)
    {
        if (!_active) return;
        if (area.IsInGroup("PlayerHitbox"))
        {
            
            _crystalHealth -= ((Hitbox)area).Damage;
            _currBossSprite.Material = _flashShaderMaterial;
            _flashTimer.Start();
            Sfx.I.Play2D(_bossHitSounds, GlobalPosition, -10);
            if (_crystalHealth <= 0)
            {
                _active = false;
                BossStartDie();
                return;
            }
            GameManager.Instance.Cam.Shake(4);
        }
    }

    private void BossStartDie()
    {
        Sfx.I.Play2D(_bossDeathSounds, GlobalPosition);
        GameManager.Instance.Cam.Shake(20, 2f);
        GetTree().CreateTimer(2f).Timeout += BossEndDie;
        _oozeParticles.Emitting = true;
    }

    private void BossEndDie()
    {
        Sfx.I.Play2D(_bossDeathSounds, GlobalPosition);
        _destructorBody.QueueFree();
        _explosionParticles.Emitting = true;
        _light.Energy = _explosionLightMaxIntensity;
        _explosionParticles.Finished += QueueFree;
        EmitSignal(SignalName.SpawnerDestroyed, GlobalPosition);
        if (_ally != null)
            _ally.FreeFromCage();
    }

    private void SpawnAlly()
    {
        var speciesList = GameManager.Instance.Allies;
        if (speciesList.Count == 0) return; // Don't spawn ally if empty
        string species = speciesList[0];
        speciesList.RemoveAt(0);

		_ally = _allyPackedScene.Instantiate<Ally>();
		_ally.Species = species;
		
		var allyInstances = GameManager.Instance.AllyInstances;
		allyInstances.Add(_ally);

        var center = GlobalPosition;
        float distanceMultiplier = 2.0f;
        var offset = new Vector2(_spawnRadius.X, _spawnRadius.Y);
        offset *= distanceMultiplier;
        _ally.GlobalPosition = center + offset;
        GameManager.Instance.PersistentNodes.AddChild(_ally);
    }
}
