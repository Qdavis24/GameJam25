using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts;

public class XpPool
{
    private float _spawnOffset = 10f;
    private PackedScene _xpOrbPckdScene;
    private Queue<Xp> _orbPool;

    public XpPool(PackedScene xpOrbPckdScene, int poolSize, Node2D parent)
    {
        _orbPool = new Queue<Xp>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var newOrb = xpOrbPckdScene.Instantiate<Xp>();
            parent.AddChild(newOrb);
            _orbPool.Enqueue(newOrb);
        }
    }

    public void SpawnXpAt(int amount, Vector2 globalPosition)
    {
        var radIncr = Mathf.Tau / amount;
        for (int i = 0; i < amount; i++)
        {
            Xp newOrb;
            if (_orbPool.Count == 0)
                newOrb = _xpOrbPckdScene.Instantiate<Xp>();
            else
                newOrb = _orbPool.Dequeue();
            
            newOrb.Enable();
            newOrb.GlobalPosition = globalPosition + Vector2.FromAngle(radIncr * i) * _spawnOffset;
        }
    }

    public void ReturnXp(Xp xp)
    {
        xp.Disable();
        _orbPool.Enqueue(xp);
    }

    public void Cleanup()
    {
        while (_orbPool.Count > 0)
        {
            _orbPool.Dequeue().QueueFree();
        }
    }
}