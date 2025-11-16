using System;
using Godot;
using System.Collections.Generic;
using GameJam25.scripts.damage_system;

public partial class EnemySpawner : Node2D
{
    [Signal] public delegate void SpawnerDestroyedEventHandler(Vector2 pos);
    
    [ExportCategory("Spawner Crystal")] 
    [Export] private float _maxCrystalHealth;
    [Export] private Area2D _crystalHurtbox;
    [Export] private AnimatedSprite2D _crystalSprite;
    [Export] private ShaderMaterial _flashShaderMaterial;
    [Export] private Timer _flashTimer;
    
    [ExportCategory("Light")]
    [Export] private PointLight2D _light;
    [Export] private Curve _lightRamp;
    [Export] private float _maxEnergy;

    [ExportCategory("Spawner Config")] 
    [Export] private PackedScene[] _enemyTypes;
    [Export] private float[] _enemyTypeSpawnChance;
    [Export] private Timer _timer;
    [Export] private Vector2 _spawnRadius;
    [Export] private int _numEnemiesPerWave = 1;
    [Export] private int _numWaves;

    private float _crystalHealth;
    private double _currTime;
    private int _currWave;

    public void Init(float maxCrystalHealth, float interval, float enemiesPerWave, float numWaves)
    {
        _crystalHealth = maxCrystalHealth;
        _numEnemiesPerWave = (int) enemiesPerWave;
        _numWaves = (int) numWaves;
        _timer.WaitTime = interval;
        
    }
    public override void _Ready()
    {
        OnTimeout();
        _currWave = 0;
        _light.Energy = 0;
        _crystalHurtbox.AreaEntered += OnCrystalHurtboxEntered;
        _timer.Timeout += OnTimeout;
        _flashTimer.Timeout += () => _crystalSprite.Material = null;
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
        for (int i = 0; i < _enemyTypes.Length ; i++)
        {
            cumProb += _enemyTypeSpawnChance[i];
            if (GD.Randf() < cumProb)
            {
                SpawnGroup(_enemyTypes[i], _numEnemiesPerWave);
                return;
            }
        }
    }

    private void SpawnGroup(PackedScene enemyPackedScene, int numEnemies)
    {
        for (int i = 0; i < numEnemies; i++)
        {
            InstantiateEnemy(enemyPackedScene);
        }
    }
    private void InstantiateEnemy(PackedScene enemyPackedScene)
    {
        var enemy = enemyPackedScene.Instantiate<Node2D>();
        var randSign = GD.Randf() < .5 ? -1 : 1;
        var xOffset = _spawnRadius.X * GD.Randf() * randSign;
        randSign = GD.Randf() < .5 ? -1 : 1;
        var yOffset = _spawnRadius.Y * GD.Randf() * randSign;
        enemy.Position += new Vector2(xOffset, yOffset);
        AddChild(enemy);
    }

    private void OnCrystalHurtboxEntered(Area2D area)
    {
        if (area.IsInGroup("PlayerHitbox"))
        {
            _crystalHealth -= ((Hitbox)area).Damage;
            _crystalSprite.Material = _flashShaderMaterial;
            _flashTimer.Start();
            if (_crystalHealth <= 0)
            {
                EmitSignal(SignalName.SpawnerDestroyed, GlobalPosition);
                QueueFree();
            }
        }
       
    }
}
