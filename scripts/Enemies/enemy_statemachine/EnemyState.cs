using Godot;
using System;
using System.Buffers;

public abstract partial class EnemyState : Node
{
    public EnemyStateMachine StateMachine;
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update(double delta);
    public abstract void PhysicsUpdate(double delta);
}