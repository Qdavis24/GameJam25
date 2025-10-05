using Godot;
using System;

public abstract partial class EnemyState : Node
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update(double delta);
    public abstract void PhysicsUpdate(double delta);
}
