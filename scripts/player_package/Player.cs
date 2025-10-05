using Godot;
using System;

public partial class Player : CharacterBody2D
{
	
	[Export] public float Speed = 400f;
	[Export] public string AnimationSet = "fox";
	
	[Export] public AudioStream WalkSfx;
	
	[Export] public PackedScene SlashAttack;
	[Export] private float LungeSpeed = 700f;
	[Export] private float LungeDuration = 0.35f;

	private bool lunging = false;
	private float lungeTime = 0f;
	private Vector2 lungeDir = Vector2.Right;
	
	private AnimatedSprite2D _anim;
	private Sprite2D _sprite;
	private AudioStreamPlayer _audio;
	
	private void Slash(Vector2 dir)
	{
		lunging = true;
		lungeTime = LungeDuration;
		lungeDir = dir;
		Velocity = dir * LungeSpeed;

		var slash = SlashAttack.Instantiate<SlashAttackNode2d>();
		slash.Position = dir.Normalized() * 10f; // a little in front of char
		slash.Direction = dir;
		AddChild(slash);
	}

	private void Move(Vector2 moveDir) {
		float offset = 8.130104f; // isometric diagonal offset

		 // Nudge diagonal movement to match isometric tile perspective
		 if (moveDir.X != 0 && moveDir.Y != 0)
		 {
		 	bool up = moveDir.Y < 0;
		 	bool right = moveDir.X > 0;
		
		 	// Map the four diagonals to ±offset:
		 	// up-right  -> +offset
		 	// up-left   -> -offset
		 	// down-left -> +offset
		 	// down-right-> -offset
		 	float angleOffset =
		 		(right ? -1f : 1f) * (up ? -1f : 1f) * Mathf.DegToRad(offset);
		
		 	moveDir = moveDir.Rotated(angleOffset).Normalized();
		 }

		// Slow down vertical only movement
		float speed = Speed;
		if (moveDir.X == 0 && moveDir.Y != 0)
			speed *= 0.9f;

		// Move
		Velocity = moveDir * speed;
	}

	private void UpdateAnimation(Vector2 moveDir) {
		void Play(string animation)
		{
			if (_anim.Animation != AnimationSet + "_" + animation)
				_anim.Play(AnimationSet + "_" + animation);
		}

		// Flip sprite horizontally for left vs right movement
		if (moveDir.X != 0) _anim.FlipH = moveDir.X < 0;

		if (moveDir == Vector2.Zero) {
			Play("idle");
		}
		else {
			Sfx.I.PlayFootstep(WalkSfx, GlobalPosition);
			Play("walk");
		}
	}

	private void GetInput(double delta)
	{		
		if (lunging)
		{
			lungeTime -= (float)delta;
			Velocity = lungeDir * LungeSpeed;
			
			_anim.Play(AnimationSet + "_walk");
			
			if (lungeDir.X != 0) 
				_anim.FlipH = lungeDir.X < 0;

			if (lungeTime <= 0f)
				lunging = false;
				
			return;
		}

		if (Input.IsActionJustPressed("attack"))
		{
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 dir = (mousePos - GlobalPosition).Normalized();
			if (dir == Vector2.Zero) dir = Vector2.Right;

			Slash(dir);
			return;
		}

		Vector2 moveDir = Input.GetVector("left", "right", "up", "down");

		Move(moveDir);
		UpdateAnimation(moveDir);
	}
	
	public override void _Ready()
	{
		_audio = GetNode<AudioStreamPlayer>("WalkSound");
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_anim.Play(AnimationSet + "_idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput(delta);
		MoveAndSlide();
	}
}
