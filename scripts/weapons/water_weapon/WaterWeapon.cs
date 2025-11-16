using Godot;
using System;
using System.Threading;
using GameJam25.scripts.weapons.base_classes;
using GameJam25.scripts.weapons.water_weapon;
using Timer = Godot.Timer;

public partial class WaterWeapon : WeaponBase
{
    [Export] private PackedScene _waterPackedScene;

    private Timer _timer;
    
    private float[] _dropAngles;
    private float _offset = 80.0f;


    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _timer.Timeout += OnTimeout;
    }


    public override void InitWeapon()
    {
        _timer.Stop();
        _dropAngles = new float[(int)_projCount];
        GetStartingAngles();
        _timer.WaitTime = _projCooldown;
        _timer.Start();
        OnTimeout();
    }

    private void GetStartingAngles()
    {
        var angleBetweenDrops = Mathf.Tau / (int)_projCount;
        var currRad = 0.0f;
        for (int i = 0; i < (int)_projCount; i++)
        {
            _dropAngles[i] = currRad;
            currRad += angleBetweenDrops;
        }
    }

    public void OnTimeout()
    {
        var fireIntervalTime = (_projCooldown - 1.0) / (int)_projCount;
        for (int i = 0; i < (int)_projCount; i++)
        {
            var spawnDir = Vector2.FromAngle(_dropAngles[i]);

            var newDrop = _waterPackedScene.Instantiate<Water>();
            newDrop.Position += spawnDir * _offset;
            AddChild(newDrop);
            newDrop.Sprite.Rotation = _dropAngles[i];
            newDrop.Timer.WaitTime = fireIntervalTime * (i + 1);
            newDrop.Timer.Start();
            newDrop.Damage = _projDamage;
            newDrop.Scale *= _projSize;
            newDrop.Speed = _projSpeed;
        }
    }
}