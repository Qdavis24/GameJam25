using GameJam25.scripts.damage_system;
using Godot;


public partial class Fireball : Node2D
{
    [Export] private PackedScene _explosionParticles;
    [Export] private Hitbox _hitbox;

    public float Damage
    {
        get
        {
            return _hitbox.Damage;
        }
        set
        {
            _hitbox.Damage = value;
        }
    }

    public override void _Ready()
    {
        _hitbox.AreaEntered += OnAreaEntered;
        _hitbox.BodyEntered += OnBodyEntered;
    }

    private void SpawnExplosion()
    {
        var explodeParticles = _explosionParticles.Instantiate<GpuParticles2D>();
        explodeParticles.Emitting = true;
        explodeParticles.Finished += () => explodeParticles.QueueFree();
        GetTree().Root.CallDeferred("add_child",  explodeParticles);
        explodeParticles.GlobalPosition = GlobalPosition;
    }
    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("EnemyHurtbox"))
        {
            SpawnExplosion();
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Obstacle"))
        {
            SpawnExplosion();
            QueueFree();
        }
    }

}