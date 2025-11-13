using Godot;
using GameJam25.scripts.damage_system;
using GameJam25.scripts.enemy_state_machines.base_classes;

public partial class Enemy : CharacterBody2D
{
    [ExportCategory("stats")] [Export] private int _health; //starting health

    [ExportCategory("Distance Ranges")] 
    [Export] public Area2D SteeringRange;
    [Export] public int AttackRange;

    [ExportCategory("Miscellaneous")] 
    [Export] public AnimatedSprite2D Animations;
    [Export] private Timer _flashTimer;
    [Export] private EStateMachine _stateMachine;
    [Export] private ShaderMaterial _flashShader;

    public float Health;


    public void TakeDamage(int amount, int knockbackWeight, Vector2 direction)
    {
        if (_stateMachine.CurrState.Name == "DeathState") return;
        Health -= amount;
        _stateMachine.InstanceContext.KnockbackDir = direction.Normalized();
        _stateMachine.InstanceContext.KnockbackWeight = knockbackWeight;
        _stateMachine.TransitionTo("KnockbackState");
    }

    public override void _Ready()
    {
        _flashTimer.Timeout += () => Animations.Material = null;
        Health = _health;
    }


    private void OnEnemyHurtBoxEntered(Area2D area)
    {
        if (!area.IsInGroup("PlayerHitbox")) return;

        Hitbox hb = (Hitbox)area;
        Animations.Material = _flashShader;
        _flashTimer.Start();
        


        TakeDamage(hb.Damage, hb.KnockbackWeight, (GlobalPosition - hb.GlobalPosition));
    }
}