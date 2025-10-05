using Godot;

public partial class PlayerCamera : Camera2D
{
	public override void _Ready()
	{
		PositionSmoothingEnabled = true;
		PositionSmoothingSpeed = 6f;

		Zoom = new Vector2(1.4f, 1.4f);
	}
}
