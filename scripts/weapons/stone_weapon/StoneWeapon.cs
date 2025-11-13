using System.Collections.Generic;
using Godot;


public partial class StoneWeapon : Node2D
{
    [Export] private PackedScene _stonePackedScene;
    [Export] private Timer _timer;
    [Export] private float _projSpeed; // pxls traveled along circumfrence of cirlce (arc length)
    [Export] private int _projCount;
    [Export] private float _offset = 50.0f;

    private Stone[] _stones;
    private float[] _stoneAngles;
    private Queue<int> _destroyedStones;
    private float _angleBetweenStones;


    private void InitWeapon()
    {
        _stones = new Stone[_projCount];
        _stoneAngles = new float[_projCount];
        _destroyedStones = new Queue<int>();
        
        for (int i = 0; i < _projCount; i++) _destroyedStones.Enqueue(i);
        _angleBetweenStones = (2 * Mathf.Pi) / _projCount;
        
        GetStartingAngles();
    }

    private void GetStartingAngles()
    {
        var currRad = 0.0f;
        for (int i = 0; i < _projCount; i++)
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
        
        for (int i = 0; i < _projCount; i++)
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