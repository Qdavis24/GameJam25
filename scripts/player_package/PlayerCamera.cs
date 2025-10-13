using Godot;

public partial class PlayerCamera : Camera2D
{
	[Export] private Node2D _target;
	[Export] private float _speed;
	public override void _Ready()
	{
		PositionSmoothingEnabled = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition = GlobalPosition.Lerp(_target.GlobalPosition, _speed);
	}
}
