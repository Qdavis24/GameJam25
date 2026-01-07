using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts;

public partial class XpPool : Node2D
{
    [Export] private float _spawnOffset = 10f;
    [Export] int _poolSize = 100;
    [Export] private PackedScene _xpOrbPckdScene;
    private Queue<Xp> _orbPool;
    private List<Xp> _allXps;

    public override void _Ready()
    {
        _orbPool = new Queue<Xp>(_poolSize);
        _allXps = new List<Xp>(_poolSize);
        for (int i = 0; i < _poolSize; i++)
        {
            var newOrb = _xpOrbPckdScene.Instantiate<Xp>();
            newOrb.Disable();
            AddChild(newOrb);
            _orbPool.Enqueue(newOrb);
            _allXps.Add(newOrb);
        }
    }

    public void SpawnXpAt(int amount, Vector2 globalPosition)
    {
        var radIncr = Mathf.Tau / amount;
        for (int i = 0; i < amount; i++)
        {
            
            if (_orbPool.Count == 0)
            {
                GD.Print("XpPool::SpawnXpAt(): orbPool is empty");
                return;
            }

            Xp newOrb = _orbPool.Dequeue();

            newOrb.Enable();
            newOrb.GlobalPosition = globalPosition + Vector2.FromAngle(radIncr * i) * _spawnOffset;
        }
    }

    public void ReturnXp(Xp xp)
    {
        xp.Disable();
        _orbPool.Enqueue(xp);
    }

    public void ReturnAll()
    {
        foreach (Xp xp in _allXps)
        {
            if (xp.InPool) continue;
            ReturnXp(xp);
        }
    }
}