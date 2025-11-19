using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.weapons.cloud_weapon;

public partial class Cloud : Node2D
{
    [Export] private Vector2 _verticalOffset = new Vector2(0, -200);
    [Export] private Timer _attackInterval;
    [Export] private float _attackRange;
    [Export] private int _lightningSegments = 5;
    [Export] private float _lightningJaggedness = 30f;
    [Export] private float _lightningWidth = 3f;
    [Export] private float _lightningDuration = 0.2f;
    [Export] private PackedScene _lightningExplodeScene;
    [Export] private float _oscillationDistance = 25f;
    [Export] private float _oscillationMagnitude = .6f;

    public Node2D Target;
    public float Speed;
    public float Damage;

    private float _distanceTraveled;
    private bool _canAttack;
    public override void _Ready()
    {
        _distanceTraveled = GD.Randf() * _oscillationDistance;
        _attackInterval.Timeout += OnTimeout;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!IsInstanceValid(Target))
        {
            QueueFree();
            return;
        }

        var dir = Target.GlobalPosition + _verticalOffset - GlobalPosition;
        if (dir.LengthSquared() <= Mathf.Pow(_attackRange, 2))
        {
            _canAttack = true;
        }
        else
        {
            _canAttack = false;
            dir = dir.Normalized();
            _distanceTraveled += dir.Length();
            
            var interpDir = (dir + dir.Orthogonal() * Mathf.Sin(_distanceTraveled/_oscillationDistance) * _oscillationMagnitude).Normalized(); 
            GlobalPosition += interpDir * Speed * (float)delta;
        }
        
    }

    private void SpawnLightning()
    {
        var lightning = new Line2D();
        lightning.Width = _lightningWidth;
        lightning.DefaultColor = Colors.White;
        lightning.ZIndex = 100; // Render on top

        // Start at cloud
        lightning.AddPoint(new Vector2(0, 0));

        // Add jagged points in between
        Vector2 direction = (Target.GlobalPosition - GlobalPosition) + new Vector2(0, -20);
        for (int i = 1; i < _lightningSegments; i++)
        {
            var t = i / (float)_lightningSegments;
            var midPoint = direction * t;
            var offset = new Vector2(
                GD.Randf() * _lightningJaggedness * 2 - _lightningJaggedness,
                GD.Randf() * _lightningJaggedness * 2 - _lightningJaggedness
            );
            lightning.AddPoint(midPoint + offset);
        }

        // End at target
        lightning.AddPoint(direction);

        AddChild(lightning);

        // Flash and fade out
        var tween = CreateTween();
        tween.TweenProperty(lightning, "modulate:a", 0f, _lightningDuration);
        tween.TweenCallback(Callable.From(() => lightning.QueueFree()));

        var explode = _lightningExplodeScene.Instantiate<LightningExplode>();
        explode.Damage = Damage;
        explode.Position = direction;
        AddChild(explode);
    }

    public void OnTimeout()
    {
        if (!_canAttack) return;
        SpawnLightning();
        
    }
}