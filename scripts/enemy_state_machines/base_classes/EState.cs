using Godot;

namespace GameJam25.scripts.enemy_state_machines.base_classes;

public abstract partial class EState : Node2D
{
    [Export] public StateName Name;
    
    protected EStateMachine _stateMachine;

    public override void _Ready()
    {
        _stateMachine = (EStateMachine)GetParent();
    }

    public abstract void Enter();

    public abstract void Exit();

    public abstract void Update(double delta);

    public abstract void PhysicsUpdate(double delta);
}