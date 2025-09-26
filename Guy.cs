using Godot;
using System;

public partial class Guy : RigidBody2D
{
    [Export] int speed;

    Vector2 input_dir;
    private void handleInput()
    {
        input_dir = Input.GetVector("left", "right", "up", "down");
    }
    public override void _Process(double delta)
    {
        float f_delta = (float)delta;
        handleInput();
        LinearVelocity = input_dir * speed;
    }
}
