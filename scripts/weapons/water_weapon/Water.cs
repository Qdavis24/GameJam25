using System;
using GameJam25.scripts.damage_system;
using Godot;

namespace GameJam25.scripts.weapons.water_weapon;

public partial class Water : Node2D
{
    [ExportCategory("Speed Behavior")] 
    [Export] private Curve _speedRamp;
    [Export] private float _timeToMaxSpeed = 2.0f;

    [ExportCategory("General Behavior")] 
    [Export] private float _loseControlDistance;


    public float Speed;

    public Timer Timer;
    public AnimatedSprite2D Sprite;

    private GpuParticles2D _explosionParticles;
    private Hitbox _hitbox;
    private Area2D _lockOnRange;

    private bool _active;
    private float _currTime;
    private Vector2 _launchDir;


    public float Damage
    {
        get { return _hitbox.Damage; }
        set { _hitbox.Damage = value; }
    }

    public override void _Ready()
    {
        Timer = GetNode<Timer>("Timer");
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hitbox = GetNode<Hitbox>("Hitbox");
        _lockOnRange = GetNode<Area2D>("LockOnRange");
        _hitbox.Monitorable = false;
        _hitbox.Monitoring = false;
        _lockOnRange.Monitoring = false;
        _hitbox.AreaEntered += OnAreaEntered;
        _hitbox.BodyEntered += OnBodyEntered;
        _lockOnRange.BodyEntered += OnLockOnRangeEntered;
        Timer.Timeout += OnTimeout;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active) return;
        
        if (_launchDir != Vector2.Zero)
        {
            GlobalPosition += _launchDir * Speed * (float)delta;
            return;
        }

        var mouseDir = GetLocalMousePosition();
        if (mouseDir.LengthSquared() < Mathf.Pow(_loseControlDistance, 2)) _launchDir = mouseDir.Normalized();
        _currTime = Math.Clamp(_currTime + (float)delta, 0, _timeToMaxSpeed);
        var currSpeed = _speedRamp.Sample(_currTime / _timeToMaxSpeed) * Speed;
        Sprite.Rotation = mouseDir.Angle();
        GlobalPosition += mouseDir.Normalized() * currSpeed * (float)delta;
    }

    public void OnTimeout()
    {
        Reparent(GetTree().Root);
        _active = true;
        _hitbox.Monitorable = true;
        _hitbox.Monitoring = true;
        _lockOnRange.Monitoring = true;
    }

    public void OnAreaEntered(Area2D area)
    {
        if (!_active) return;
        if (!area.IsInGroup("EnemyHurtbox")) return;
        QueueFree();
    }

    public void OnBodyEntered(Node2D body)
    {
        if (!_active) return;
        if (!body.IsInGroup("Obstacle")) return;
        QueueFree();
    }

    public void OnLockOnRangeEntered(Node2D body)
    {
        if (!body.IsInGroup("Enemies")) return;
        _launchDir = (body.GlobalPosition - GlobalPosition).Normalized();
        Sprite.Rotation = _launchDir.Angle();
    }
}