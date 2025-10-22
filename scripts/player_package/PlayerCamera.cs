using Godot;

public partial class PlayerCamera : Camera2D
{
    [Export] private float _speed;

    public Node2D Target;

    public override void _Ready()
    {
        PositionSmoothingEnabled = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Target != null)
        {
            GD.Print("yes");
            GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition, _speed);
        }
    }
}