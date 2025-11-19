using Godot;
using System;
using GameJam25.scripts.damage_system;

public partial class LightningExplode : Node2D
{
    [Export] private Hitbox _hitbox;
    [Export] private GpuParticles2D _particles;

    public float Damage
    {
        set
        {
            _hitbox.Damage = value;
        }
    }

    public override void _Ready()
    {
        _particles.Emitting = true;
        _particles.Finished += QueueFree;
    }
}
