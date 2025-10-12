using Godot;
using System;

public partial class FireballWeapon : Node2D
{
    [Export] private PackedScene _fireballScene;
    [Export] private Timer _timer;
    [Export] private float _fireballSpeed;
    [Export] private Curve _speedRamp;
    [Export] private double _fireballLifetime;
    private double _currTime;

    private Vector2[] _directions =
    {
        new Vector2(-1, 0),
        new Vector2(-1, -1).Normalized(),
        new Vector2(0, -1),
        new Vector2(1, -1).Normalized(),
        new Vector2(1, 0),
        new Vector2(1, 1).Normalized(),
        new Vector2(0, 1),
        new Vector2(-1, 1).Normalized()
    };

    private bool _isLaunching;
    private Node2D[] _fireballs;

    public override void _Ready()
    {
        _timer.Timeout += OnTimeout;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isLaunching)
        {
            _currTime += delta;
            for (int i = 0; i < _fireballs.Length; i++)
            {
                if (IsInstanceValid(_fireballs[i]))
                {
                    Vector2 dir = _directions[i] * _speedRamp.Sample((float)(_currTime / _fireballLifetime)) *
                                  _fireballSpeed;
                    _fireballs[i].GlobalPosition += dir * (float)delta;
                }
            }
        }
    }

    private void CleanupBalls()
    {
        foreach (Node2D fireball in _fireballs)
        {
            if (IsInstanceValid(fireball))
                fireball.QueueFree();
        }
    }

    private void OnTimeout()
    {
        _currTime = 0;
        if (_fireballs != null)
            CleanupBalls();
        _fireballs = new Node2D[_directions.Length];
        for (int i = 0; i < _directions.Length; i++)
        {
            var currFireball = _fireballScene.Instantiate<Node2D>();
            GetTree().Root.AddChild(currFireball);
            currFireball.GlobalPosition = GlobalPosition;
            currFireball.Rotation = (_directions[i].Angle() + Mathf.Pi / 3);
            _fireballs[i] = currFireball;
        }

        _isLaunching = true;
    }
}