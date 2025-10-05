using Godot;

public abstract partial class Enemy : Node2D
{
    [Export] float health;
    [Export] public Area2D AggroRange;
    [Export] public string[] AggroGroups;
    [Export] public Curve ChasePath;
    [Export] public int Speed;
    [Export] public AnimatedSprite2D animations;
    
    protected float currHealth;
    

    public void TakeDamage(int amount)
    {
        health -= amount;
    }

    private void Die()
    {
        QueueFree();
    }

    public override void _Ready()
    {
        currHealth = health;
    }
    public override void _Process(double delta)
    {
        if (health < 0) Die();
    }
    
}