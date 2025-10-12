using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = Godot.Vector2;

public partial class FireballWeapon : Node2D
{
    [Export] private PackedScene _fireballPackedScene;
    [Export] private Timer _timer;
    [Export] private float _fireballSpeed;
    [Export] private Curve _speedRamp;
    [Export] private double _fireballLifetime;
    [Export] private int _numFireballs;


    private enum _states
    {
        Cooldown,
        Fire
    };

    private _states _currState;

    private Node2D[] _fireballs;
    private Vector2[] _directions;

    private List<Node2D[]> _burstsQueue;
    private List<double> _currTimes;

    private void calculateDirections()
    {
        float radIncr = (2 * Mathf.Pi) / _numFireballs;
        float currRad = 0;
        for (int i = 0; i < _numFireballs; i++)
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
        _burstsQueue = new List<Node2D[]>();
        _currTimes = new List<double>();

        _directions = new Vector2[_numFireballs];
        calculateDirections();

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
            for (int j = 0; j < _numFireballs; j++)
            {
                if (IsInstanceValid(_burstsQueue[i][j]))
                {
                    Vector2 dir =
                        _directions[j].Rotated((float)((_currTimes[i] / _fireballLifetime * (2 * Mathf.Pi)))) *
                        _speedRamp.Sample((float)(_currTimes[i] / _fireballLifetime)) *
                        _fireballSpeed;
                    _burstsQueue[i][j].Rotation = dir.Angle();
                    _burstsQueue[i][j].GlobalPosition += dir * (float)delta;
                }
            }
        }
        
    }

    private void OnTimeout()
    {
        GD.Print("Timeout hit");
        _currTimes.Add(0);

        _fireballs = new Node2D[_numFireballs];
        for (int i = 0; i < _numFireballs; i++)
        {
            var currFireball = _fireballPackedScene.Instantiate<Node2D>();
            currFireball.Rotation = _directions[i].Angle();
            AddChild(currFireball);
            currFireball.GlobalPosition = GlobalPosition;
            _fireballs[i] = currFireball;
        }

        _burstsQueue.Add(_fireballs);
        GD.Print(_burstsQueue.Count);
    }
}