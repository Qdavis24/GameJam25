using System.Collections;
using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts;

public partial class PickupPool : Node2D
{
    [Export] private float _spawnOffset = 10f;
    [Export] int _poolSize = 100;
    [Export] private PackedScene _xpPckdScene;
    [Export] private PackedScene _healthPckdScene;
    
    private PickupType[] _types = { PickupType.Xp, PickupType.Health };
    private Dictionary<PickupType, Queue<Pickup>> _pool;
    private Dictionary<PickupType, PackedScene> _pickupScenes;
    private List<Pickup> _allPickups;

    public override void _Ready()
    {
        _pool = new()
        {
            { PickupType.Xp, new Queue<Pickup>(_poolSize) },
            { PickupType.Health, new Queue<Pickup>(_poolSize) }
        };
        _pickupScenes = new()
        {
            { PickupType.Xp, _xpPckdScene },
            { PickupType.Health, _healthPckdScene }
        };
        _allPickups = new List<Pickup>(_poolSize * 2); // num of types of pickups could make this better
        foreach (PickupType type in _types)
            for (int i = 0; i < _poolSize; i++)
            {
                var newPickup = _pickupScenes[type].Instantiate<Pickup>();
                newPickup.Disable();
                AddChild(newPickup);
                _pool[type].Enqueue(newPickup);
                _allPickups.Add(newPickup);
            }
    }

    public void SpawnPickupAt(PickupType type, Vector2 globalPosition, int amount = 1)
    {
        var radIncr = Mathf.Tau / amount;
        for (int i = 0; i < amount; i++)
        {
            if (_pool[type].Count == 0)
            {
                GD.Print($"PickupPool::SpawnPickupAt(): {type} pool is empty");
                return;
            }

            Pickup newPickup = _pool[type].Dequeue();

            newPickup.Enable();
            newPickup.GlobalPosition = globalPosition + Vector2.FromAngle(radIncr * i) * _spawnOffset;
        }
    }

    public void ReturnPickup(Pickup pickup)
    {
        pickup.Disable();
        _pool[pickup.Type].Enqueue(pickup);
    }

    public void ReturnAll()
    {
        foreach (Pickup pickup in _allPickups)
        {
            if (pickup.InPool) continue;
            ReturnPickup(pickup);
        }
    }
}