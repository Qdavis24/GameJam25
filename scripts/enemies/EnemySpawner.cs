using Godot;
using System.Collections.Generic;
using GameJam25.scripts.state_machine;

public partial class EnemySpawner : Node2D
{
    [ExportCategory("Light")]
    [Export] private PointLight2D _light;
    [Export] private Curve _lightRamp;
    [Export] private float _maxEnergy;

    [ExportCategory("Spawner Config")] [Export]
    private Area2D _toggleRange;
    [Export] private PackedScene[] _enemyTypes;
    [Export] private float[] _enemyTypeSpawnChance;
    [Export] private Timer _timer;
    [Export] private Vector2 _spawnRadius;
    [Export] private int Count = 1;
    
    private double _currTime = 0;
    private int _currCount = 0;
    public override void _Ready()
    {
        _light.Energy = 0;
        _timer.Timeout += Spawn;
        _toggleRange.BodyEntered += OnToggleRangeEntered;
        _toggleRange.BodyExited += OnToggleRangeExited;
    }

    public override void _Process(double delta)
    {
        _currTime += delta;
        var energySample = _lightRamp.Sample((float)(_currTime / _timer.WaitTime));
        _light.Energy = energySample * _maxEnergy;
    }

    private void Spawn()
    {
        _currTime = 0;
        if (_currCount > Count) return;
        _currCount++;
        float cumProb = 0;
        for (int i = 0; i < _enemyTypes.Length ; i++)
        {
            cumProb += _enemyTypeSpawnChance[i];
            if (GD.Randf() < cumProb)
            {
                InstantiateEnemy(_enemyTypes[i]);
            }
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

    private void OnToggleRangeEntered(Node2D body)
    {
        if (body.IsInGroup("Players"))
        {
            _timer.Start();
        }
    }
    
    private void OnToggleRangeExited(Node2D body)
    {
        if (body.IsInGroup("Players"))
        {
            _timer.Stop();
        }
    }
}
