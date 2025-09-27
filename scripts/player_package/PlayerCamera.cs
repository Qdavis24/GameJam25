using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
	public override void _Ready()
	{
		PositionSmoothingEnabled = true;
		PositionSmoothingSpeed = 6f;
	}
}
