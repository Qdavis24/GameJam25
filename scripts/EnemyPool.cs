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
    private List<Enemy> _allEnemies;

    private Dictionary<EnemyType, Queue<Enemy>> _pool;

    public override void _Ready()
    {
        _allEnemies = new List<Enemy>();
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
            for (int i = 0; i < _poolSize; i++)
            {
                var currEnemy = CreateEnemyForPool(_enemyScenes[type]);
                _pool[type].Enqueue(currEnemy);
            }
        }
    }

    private Enemy CreateEnemyForPool(PackedScene scene)
    {
        var newEnemy = scene.Instantiate<Enemy>();
        AddChild(newEnemy);
        newEnemy.Disable();
        _allEnemies.Add(newEnemy);
        return newEnemy;
    }

    public void SpawnEnemyAt(EnemyType type, Vector2 globalPosition)
    {
        if (!_pool.ContainsKey(type)) GD.PrintErr("EnemyPool.SpawnEnemyAt : type doesn't exist");
        Enemy newEnemy;
        if (_pool[type].Count == 0)
        {
            GD.PrintErr("EnemyPool::SpawnEnemyAt ${type} enemy pool empty");
            return;
        }
        newEnemy = _pool[type].Dequeue();
        newEnemy.Enable(globalPosition);
    }

    public bool ReturnEnemy(Enemy enemy)
    {
        if (!IsInstanceValid(enemy))
        {
            GD.PrintErr($"EnemyPool::ReturnEnemy Enemy Returning to Pool is not valid instance ${enemy.Name}");
            return false;
        }
        var type = enemy.Type;
        if (!_pool.ContainsKey(type)) GD.PrintErr("EnemyPool::ReturnEnemy Type doesn't exist");
        enemy.Disable();
        _pool[type].Enqueue(enemy);
        return true;
    }

    public void ReturnAllEnemies()
    {
        foreach (Enemy enemy in _allEnemies)
        {
            if (enemy.InPool) continue;
            ReturnEnemy(enemy);
        }
    }
}