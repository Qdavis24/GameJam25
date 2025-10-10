using GameJam25.scripts.state_machine.enemy_states;
using Godot;

namespace GameJam25.scripts.state_machine;

public abstract partial class State : Node
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update(double delta);
    public abstract void PhysicsUpdate(double delta);
}