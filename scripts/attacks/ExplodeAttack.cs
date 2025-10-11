using Godot;
using System;

public partial class ExplodeAttack : Node2D
{
    [Signal]
    public delegate void AttackFinishedEventHandler();
    [Export] private GpuParticles2D _explodeEffect;
    [Export] private Node2D Owner;

    public override void _Ready()
    {
        _explodeEffect.Emitting = true;
        _explodeEffect.Finished += OnFinished;
    }

    private void OnFinished()
    {
        GD.Print("Attack Scene Callback finished");
        EmitSignal(SignalName.AttackFinished);
        QueueFree();
    }
}
