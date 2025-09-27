using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 400f;

	// Tile pixel size for adjusted diagonal angle movement
	[Export] public float TileWidth = 120f;
	[Export] public float TileHeight = 90f;
	
	private AnimatedSprite2D _anim;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		//_anim.Play("idle");
		_anim.Play("down_right");
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
		// if (input.X != 0) _sprite.FlipH = input.X < 0;
		
		if (input != Vector2.Zero)
		{
			// Decide which animation to play
			if (input.Y < 0)
			{
				if (_anim.Animation != "up_right")
					_anim.Play("up_right");
			}
			else if (input.Y > 0)
			{
				if (_anim.Animation != "walk_down")
					_anim.Play("down_right");
			}
			else
			{
				// default to side walk animation if only left/right
				if (_anim.Animation != "down_right")
					_anim.Play("down_right");
			}

			// Flip horizontally for left vs right
			if (input.X != 0)
				_anim.FlipH = input.X < 0;
		}
		else
		{
			if (_anim.Animation != "idle")
				_anim.Play("idle");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		MoveAndSlide();
	}
}
