using GameJam25.scripts.state_machine.enemy_states;
using Godot;

namespace GameJam25.scripts.state_machine.enemy_state_machines;

public partial class EState : State
{
    
    protected EStateMachine _stateMachine;

    public override void _Ready()
    {
        _stateMachine = (EStateMachine)GetParent();
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
    }
}