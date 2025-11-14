using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameJam25.scripts.weapons.base_classes;
using Vector2 = Godot.Vector2;

public partial class FireballWeapon : WeaponBase
{
    [Export] private PackedScene _fireballPackedScene;
    [Export] private Curve _speedRamp;
    [Export] private double _fireballLifetime;
    
    private Node2D[] _fireballs;
    private Vector2[] _directions;

    private List<Node2D[]> _burstsQueue;
    private List<float> _burstsDistances;
    private List<double> _currTimes;

    private float _distancePerFrame;


    protected override void InitWeapon()
    {
        if (_burstsQueue != null)
        {
            var i = 0;
            while (_burstsQueue.Count > 0)
            {
                CleanupBalls(i);
                i++;
            }
        }
        
        _burstsQueue = new List<Node2D[]>();
        _burstsDistances = new List<float>();
        _currTimes = new List<double>();
        _directions = new Vector2[(int)_projCount];
        CalculateDirections();
    }
    private void CalculateDirections()
    {
        float radIncr = (2 * Mathf.Pi) / _projCount;
        float currRad = 0;
        for (int i = 0; i < _projCount; i++)
        {
            _directions[i] = Vector2.FromAngle(currRad);
            currRad += radIncr;
        }
    }

    private void CleanupBalls(int burstIndex)
    {
        foreach (Node2D fireball in _burstsQueue[burstIndex])
        {
            if (IsInstanceValid(fireball))
                fireball.QueueFree();
        }
        _burstsQueue.RemoveAt(burstIndex);
        _currTimes.RemoveAt(burstIndex);
    }

    public override void _Ready()
    {
        InitWeapon();

        _distancePerFrame = _directions[0].Length();
        _timer.Timeout += OnTimeout;
    }


    public override void _PhysicsProcess(double delta)
    {
        for (int i = 0; i < _burstsQueue.Count; i++)
        {
            _currTimes[i] += delta;
            // trigger deque for a burst
            if (_currTimes[i] >= _fireballLifetime)
            {
                CleanupBalls(i);
                continue;
            }

            
            // position each fireball in a burst
            for (int j = 0; j < _projCount; j++)
            {
                if (IsInstanceValid(_burstsQueue[i][j]))
                {
                    Vector2 dir =
                        _directions[j] *
                        _speedRamp.Sample((float)(_currTimes[i] / _fireballLifetime)) *
                        _projSpeed;
                    Vector2 perpDir = new Vector2(-dir.Y, dir.X);
                    dir += perpDir * (float) Math.Sin((_burstsDistances[i] / 50) * Mathf.Tau) *.5f;
                    _burstsQueue[i][j].GlobalPosition += dir * (float)delta;
                    _burstsQueue[i][j].Rotation = dir.Angle();
                }
            }

            _burstsDistances[i] += _distancePerFrame;

        }
        
    }

    private void OnTimeout()
    {
        _currTimes.Add(0);

        _fireballs = new Node2D[(int)_projCount];
        for (int i = 0; i < (int)_projCount; i++)
        {
            var currFireball = _fireballPackedScene.Instantiate<Fireball>();
            currFireball.Scale *= _projSize;
            currFireball.Damage = _projDamage;
            currFireball.Rotation = _directions[i].Angle();
            GetTree().Root.AddChild(currFireball);
            currFireball.GlobalPosition = GlobalPosition;
            _fireballs[i] = currFireball;
        }

        _burstsDistances.Add(0.0f);
        _burstsQueue.Add(_fireballs);
    }
}