using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 400f;

	// Tile pixel size for adjusted diagonal angle movement
	[Export] public float TileWidth = 120f;
	[Export] public float TileHeight = 90f;
	
	private Sprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
	}

	private void GetInput()
	{
		Vector2 input = Input.GetVector("left", "right", "up", "down");
		Vector2 moveDir = input;

		float thetaRad = Mathf.Atan2(TileHeight, TileWidth);
		float thetaDeg = Mathf.RadToDeg(thetaRad);
		float offset = 45 - thetaDeg;

		// Nudge diagonal movement to match isometric tile perspective
		if (input.X != 0 && input.Y != 0)
		{
			bool up = input.Y < 0;
			bool right = input.X > 0;

			// Map the four diagonals to ±offset:
			// up-right  -> +offset
			// up-left   -> -offset
			// down-left -> +offset
			// down-right-> -offset
			float angleOffset =
				(right ? -1f : 1f) * (up ? -1f : 1f) * Mathf.DegToRad(offset);

			moveDir = input.Rotated(angleOffset).Normalized();
		}

		// Slow down vertical movement
		float speed = Speed;
		if (input.X == 0 && input.Y != 0)
			speed *= 0.9f;

		Velocity = moveDir * speed;


		// Flip sprite horizontally based on left/right input
		if (input.X != 0) _sprite.FlipH = input.X < 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		MoveAndSlide();
	}
}
