using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;
using Godot;


public partial class StoneWeapon : WeaponBase
{
    [Export] private PackedScene _stonePackedScene;
    [Export] private float _offset = 50.0f;

    private Stone[] _stones;
    private float[] _stoneAngles;
    private Queue<int> _destroyedStones;
    private float _angleBetweenStones;

    

    private void InitWeapon()
    {
        _stones = new Stone[(int)_projCount];
        _stoneAngles = new float[(int)_projCount];
        _destroyedStones = new Queue<int>();
        
        for (int i = 0; i < (int)_projCount; i++) _destroyedStones.Enqueue(i);
        _angleBetweenStones = (2 * Mathf.Pi) / (int)_projCount;
        
        GetStartingAngles();
    }

    private void GetStartingAngles()
    {
        var currRad = 0.0f;
        for (int i = 0; i < (int)_projCount; i++)
        {
            _stoneAngles[i] = currRad;
            currRad += _angleBetweenStones;
        }
    }

    public override void _Ready()
    {
        InitWeapon();
        _timer.Timeout += OnTimeout;
    }

    public override void _PhysicsProcess(double delta)
    {
        var currRadIncrease = (_projSpeed / _offset) * delta;
        
        for (int i = 0; i < (int)_projCount; i++)
        {
            _stoneAngles[i] += (float)currRadIncrease;

            if (IsInstanceValid(_stones[i]))
            {
                var newPos = Vector2.FromAngle(_stoneAngles[i]) * _offset;
                _stones[i].Rotation = newPos.Angle();
                _stones[i].Position = newPos;
            }
        }
    }

    private void OnTimeout()
    {
        if (_destroyedStones.Count <= 0)
        {
            _timer.Stop();
            return;
        }

        var currIndex = _destroyedStones.Dequeue();

        var newStone = _stonePackedScene.Instantiate<Stone>();
        newStone.Damage = _projDamage;
        newStone.Position += Vector2.FromAngle(_stoneAngles[currIndex]) * _offset;

        _stones[currIndex] = newStone;

        newStone.StoneDestroyed += () =>
        {
            _stones[currIndex] = null;
            _destroyedStones.Enqueue(currIndex);
            if (_timer.IsStopped()) _timer.Start();
        };

        AddChild(newStone);
    }
    
}