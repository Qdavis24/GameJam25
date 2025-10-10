using Godot;
using GameJam25.scripts.player_package.hitbox;
using GameJam25.scripts.state_machine;
using GameJam25.scripts.state_machine.enemy_states;

public partial class Enemy : CharacterBody2D
{
    [ExportCategory("stats")] 
    [Export] public int Speed;
    [Export] int Health; //starting health

    [ExportCategory("Distance Ranges")] 
    [Export] public Area2D AggroRange;
    [Export] public Area2D SteeringRange;
    [Export] public int AttackRange;

    [ExportCategory("Miscellaneous")] 
    [Export] public string[] AggroGroups;
    [Export] public AnimatedSprite2D Animations;
    [Export] private EStateMachine _stateMachine;

    protected float _currHealth;


    public void TakeDamage(int amount, Vector2 direction)
    {
        _currHealth -= amount;
        _stateMachine.InstanceContext.KnockBackDir = direction.Normalized();
        _stateMachine.TransitionTo("KnockbackState");
    }

    public override void _Ready()
    {
        _currHealth = Health;
    }

    public override void _Process(double delta)
    {
        if (_currHealth < 0 && _stateMachine.CurrState.Name != "DeathState") _stateMachine.TransitionTo("DeathState");
    }

    private void OnEnemyHurtBoxEntered(Area2D area)
    {
        if (!area.IsInGroup("PlayerAttacks")) return;

        Hitbox hb = (Hitbox)area;
        TakeDamage(hb.Damage, (GlobalPosition - area.GlobalPosition));
    }
}