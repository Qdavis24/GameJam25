using Godot;

namespace GameJam25.scripts.enemies.owl_wraith_package;

public partial class ShadowBall : Node2D
{
    [Export] private PackedScene _explosionParticles;
    [Export] public Area2D Hitbox;
    public Vector2 Velocity;
    public int Speed;
    
    public override void _Ready()
    {
        Hitbox.AreaEntered += OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Velocity * Speed * (float) delta;
    }

    public void OnAreaEntered(Area2D area)
    {
        if (!area.IsInGroup("PlayerHurtbox") && !area.IsInGroup("Obstacle")) return;
        var explodeParticles = _explosionParticles.Instantiate<GpuParticles2D>();
        explodeParticles.Emitting = true;
        explodeParticles.Finished += () => explodeParticles.QueueFree();
        GetTree().Root.AddChild(explodeParticles);
        explodeParticles.GlobalPosition = GlobalPosition;
        QueueFree();
    }
    
}