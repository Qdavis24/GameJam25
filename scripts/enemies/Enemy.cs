using Godot;
using GameJam25.scripts.damage_system;
using GameJam25.scripts.enemy_state_machines.base_classes;

public partial class Enemy : CharacterBody2D
{
    [ExportCategory("stats")] 
    [Export] private int _health; //starting health

    [ExportCategory("Distance Ranges")] 
    [Export] public Area2D SteeringRange;
    [Export] public int AttackRange;

    [ExportCategory("Miscellaneous")] 
    [Export] public AnimatedSprite2D Animations;
    [Export] private EStateMachine _stateMachine;

    protected float _currHealth;


    public void TakeDamage(int amount, int knockbackWeight, Vector2 direction)
    {
        if (_stateMachine.CurrState.Name == "DeathState") return;
        _currHealth -= amount;
        _stateMachine.InstanceContext.KnockbackDir = direction.Normalized();
        _stateMachine.InstanceContext.KnockbackWeight = knockbackWeight;
        _stateMachine.TransitionTo("KnockbackState");
    }

    public override void _Ready()
    {
        _currHealth = _health;
    }

    public override void _Process(double delta)
    {
        if (_currHealth < 0 && _stateMachine.CurrState.Name != "DeathState") _stateMachine.TransitionTo("DeathState");
    }

    private void OnEnemyHurtBoxEntered(Area2D area)
    {
        if (!area.IsInGroup("PlayerHitBox")) return;

        Hitbox hb = (Hitbox)area;
        
        TakeDamage(hb.Damage, hb.KnockbackWeight,  (GlobalPosition - hb.GlobalPosition));
    }
}