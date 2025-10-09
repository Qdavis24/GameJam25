using Godot;
using GameJam25.scripts.player_package.hitbox;

public abstract partial class Enemy : CharacterBody2D
{
    [Export] float health;
    [Export] public Area2D AggroRange;
    [Export] public Area2D SteeringRange;
    [Export] public int AttackRange;
    [Export] public CpuParticles2D DeathParticles;
    [Export] public CpuParticles2D HitParticles;
    [Export] public Curve KnockBackCurve;
    public Vector2 KnockBackDir = Vector2.Zero;
    
    [Export] public string[] AggroGroups;
    [Export] public Curve ChasePath;
    
    [Export] public int Speed;
    [Export] private int KnockBack;
    [Export] public AnimatedSprite2D animations;
    public Node2D CurrentTarget;

    [Export] private EnemyStateMachine _stateMachine;
    
    protected float currHealth;
    

    public void TakeDamage(int amount, Vector2 direction)
    {
        GD.Print("YES");
        KnockBackDir = direction.Normalized();
        _stateMachine.OnStateTransition(_stateMachine.CurrState.Name, "KnockBackState");
        health -= amount;
    }
    

    public override void _Ready()
    {
        currHealth = health;
    }
    public override void _Process(double delta)
    {
        if (health < 0 && _stateMachine.CurrState.Name != "DeathState") _stateMachine.OnStateTransition(_stateMachine.CurrState.Name, "DeathState");
        
    }

    public override void _PhysicsProcess(double delta)
    {
        
        MoveAndSlide();
        
    }

    public void OnEnemyHurtBoxEntered(Area2D area)
    {
        if (area.IsInGroup("PlayerAttacks"))
        {
            Hitbox hb = (Hitbox)area; 
            TakeDamage(hb.Damage,(GlobalPosition-area.GlobalPosition));
        }
    }
    
}