using Godot;

namespace GameJam25.scripts.enemies.owl_wraith_package;

public partial class ShadowBall : Node2D
{
    [Export] private PackedScene _explosionParticles;
    [Export] private Area2D _hitbox;
    private Vector2 _velocity;
    private int _speed;
    
    public override void _Ready()
    {
        
        _hitbox.AreaEntered += OnAreaEntered;
        _hitbox.BodyEntered += OnBodyEntered;
        
    }

    public void Init(Vector2 velocity, int speed)
    {
        Rotation = velocity.Angle();
        _velocity = velocity;
        _speed = speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _velocity * _speed * (float) delta;
    }

    private void SpawnExplosion()
    {
        var explodeParticles = _explosionParticles.Instantiate<GpuParticles2D>();
        explodeParticles.Emitting = true;
        explodeParticles.Finished += () => explodeParticles.QueueFree();
        GetTree().Root.AddChild(explodeParticles);
        explodeParticles.GlobalPosition = GlobalPosition;
    }
    public void OnAreaEntered(Area2D area)
    {
        if (!area.IsInGroup("PlayerHurtbox")) return;
        SpawnExplosion();
        QueueFree();
    }

    public void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Obstacle")) return;
        SpawnExplosion();
        QueueFree();
    }
}