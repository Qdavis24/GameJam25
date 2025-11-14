using Godot;
using System;
using GameJam25.scripts.damage_system;

public partial class Stone : Node2D
{
    [Signal]
    public delegate void StoneDestroyedEventHandler();
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
