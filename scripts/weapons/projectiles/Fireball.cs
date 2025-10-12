using Godot;
using System;

public partial class Fireball : Node2D
{
    [Export] private GpuParticles2D _explode;
    [Export] private Area2D Hitbox;
    private bool _queueFree;

    public override void _Ready()
    {
        Hitbox.AreaEntered += OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_queueFree && _explode.Emitting == false) QueueFree();
    }

    private void OnAreaEntered(Node2D area)
    {
        if (area.IsInGroup("EnemyHurtbox"))
        {
            GD.Print("HIT");
            _explode.Emitting = true;
            _queueFree = true;
        }
    }
}