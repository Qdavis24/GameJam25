using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.enemies;

public partial class EnemyPool : Node2D
{
    private EnemyType[] _types = { EnemyType.BunnyWraith, EnemyType.DeerWraith, EnemyType.OwlWraith };

    [Export] private int _poolSize;

    [Export] private PackedScene _bunnyWraithScene;
    [Export] private PackedScene _owlWraithScene;
    [Export] private PackedScene _deerWraithScene;

    private Dictionary<EnemyType, PackedScene> _enemyScenes;

    private Dictionary<EnemyType, Queue<Enemy>> _pool;

    public override void _Ready()
    {
        _enemyScenes = new()
        {
            { EnemyType.BunnyWraith, _bunnyWraithScene },
            { EnemyType.DeerWraith, _deerWraithScene },
            { EnemyType.OwlWraith, _owlWraithScene }
        };
        _pool = new()
        {
            { EnemyType.BunnyWraith, new Queue<Enemy>() },
            { EnemyType.DeerWraith, new Queue<Enemy>() },
            { EnemyType.OwlWraith, new Queue<Enemy>() }
        };

        foreach (EnemyType type in _types)
        {
            for (int i = 0; i < _poolSize; i++) _pool[type].Enqueue(CreateEnemyForPool(_enemyScenes[type]));
        }
    }

    private Enemy CreateEnemyForPool(PackedScene scene)
    {
        var newEnemy = scene.Instantiate<Enemy>();
        AddChild(newEnemy);
        newEnemy.Disable();
        return newEnemy;
    }

    public void SpawnEnemyAt(EnemyType type, Vector2 globalPosition)
    {
        if (!_pool.ContainsKey(type)) GD.PrintErr("EnemyPool.SpawnEnemyAt : type doesn't exist");
        Enemy newEnemy;
        if (_pool[type].Count == 0)
        {
            newEnemy = _enemyScenes[type].Instantiate<Enemy>();
            AddChild(newEnemy);
        }
        else
            newEnemy = _pool[type].Dequeue();
        
        newEnemy.Enable(globalPosition);
    }

    public void ReturnEnemy(Enemy enemy)
    {
        var type = enemy.Type;
        if (!_pool.ContainsKey(type)) GD.PrintErr("EnemyPool.ReturnEnemy : type doesn't exist");
        enemy.Disable();
        _pool[type].Enqueue(enemy);
    }
}