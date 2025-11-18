using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameJam25.scripts.weapons.base_classes;
using Vector2 = Godot.Vector2;

public partial class FireballWeapon : WeaponBase
{
    [Export] private PackedScene _fireballPackedScene;
    [Export] private float _oscillationLength;
    [Export] private float _oscillationMag;
    
    private Timer _timer;
    
    private Node2D[] _fireballs;
    private Vector2[] _directions;
    private List<Node2D[]> _bursts;
    private List<float> _burstsDistances;


    public override void InitWeapon()
    {
        _directions = new Vector2[(int)_projCount];
        _timer.WaitTime = _projCooldown;
        CalculateDirections();
        OnTimeout();
        _timer.Start();
        _active = true;
    }
    private void CalculateDirections()
    {
        float radIncr = (2 * Mathf.Pi) / (int)_projCount;
        float currRad = 0;
        for (int i = 0; i < (int)_projCount; i++)
        {
            _directions[i] = Vector2.FromAngle(currRad);
            currRad += radIncr;
        }
    }
    
    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _timer.Timeout += OnTimeout;
        
        _bursts = new List<Node2D[]>();
        _burstsDistances = new List<float>();
    }


    public override void _PhysicsProcess(double delta)
    {
        if (!_active) return;
        
        var burstsToRemove = new List<int>();
        var distanceTraveled = _directions[0].Length() * _projSpeed * (float)delta;
        for (int i = 0; i < _bursts.Count; i++) // go through each burst
        {
            bool burstFinished = true;
            for (int j = 0; j < _bursts[i].Length; j++) // position each fireball in a single burst
            {
                if (IsInstanceValid(_bursts[i][j]))
                {
                    burstFinished = false;
                    Vector2 dir =
                        _directions[j] *
                        _projSpeed;
                    Vector2 perpDir = new Vector2(-dir.Y, dir.X);
                    dir += perpDir * (float) Math.Sin((_burstsDistances[i] / _oscillationLength) * Mathf.Tau) * _oscillationMag;
                    _bursts[i][j].GlobalPosition += dir * (float)delta;
                    _bursts[i][j].Rotation = dir.Angle();
                }
            }

            if (burstFinished) burstsToRemove.Add(i);
            _burstsDistances[i] += distanceTraveled;
        }

        foreach (int index in burstsToRemove)
        {
            _bursts.RemoveAt(index);
            _burstsDistances.RemoveAt(index);
        }
        
    }

    private void OnTimeout()
    {
        _fireballs = new Node2D[(int)_projCount];
        for (int i = 0; i < (int)_projCount; i++)
        {
            var currFireball = _fireballPackedScene.Instantiate<Fireball>();
            currFireball.GlobalPosition = GlobalPosition;
            GameManager.Instance.World.AddChild(currFireball);
            currFireball.ExplosionScale = _projSize;
            currFireball.Scale *= _projSize;
            currFireball.Damage = _projDamage;
            currFireball.Rotation = _directions[i].Angle();
            
            _fireballs[i] = currFireball;
        }

        _burstsDistances.Add(0.0f);
        _bursts.Add(_fireballs);
    }
}
