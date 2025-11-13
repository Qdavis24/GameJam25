using Godot;
using System;

public partial class Stone : Node2D
{
    [Signal]
    public delegate void StoneDestroyedEventHandler();
    [Export] private PackedScene _explosionParticles;
    [Export] private Area2D _hitbox;
    private Vector2 _velocity;
    private int _speed;
    
    public override void _Ready()
    {
        
        _hitbox.AreaEntered += OnAreaEntered;
        
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
        if (!area.IsInGroup("EnemyHurtbox")) return;
        //SpawnExplosion();
        
        QueueFree();
    }

    public override void _ExitTree()
    {
        EmitSignalStoneDestroyed();
        base._ExitTree();
    }
}
