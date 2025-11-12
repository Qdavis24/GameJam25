using Godot;
using System;
using GameJam25.scripts.damage_system;

public partial class Player : CharacterBody2D
{
	[Signal] public delegate void HealthChangedEventHandler(int newHealth);
	[Signal] public delegate void MaxHealthChangedEventHandler(int newMaxHealth);
	[Signal] public delegate void XpChangedEventHandler(int newXp);
	[Signal] public delegate void LevelChangedEventHandler(int newLevel);

	// Values used by UI for display or game logic
	// Setters implemented below to emit changes to UI.
	private int _health = 3;
	private int _maxHealth = 3;
	private int _xp = 0;
	private int _level = 1;

	// Current Health
	[Export]
	public int Health
	{
		get => _health;
		set
		{
			if (_health == value) return;
			_health = Mathf.Clamp(value, 0, MaxHealth);
			EmitSignal(SignalName.HealthChanged, _health);
		}
	}

	// Max Health
	[Export]
	public int MaxHealth
	{
		get => _maxHealth;
		set
		{
			if (_maxHealth == value) return;
			_maxHealth = Mathf.Max(1, value);
			EmitSignal(SignalName.MaxHealthChanged, _maxHealth);
		}
	}

	// Experience
	[Export]
	public int Xp
	{
		get => _xp;
		set
		{
			if (_xp == value) return;
			_xp = Mathf.Max(0, value);
			EmitSignal(SignalName.XpChanged, _xp);
		}
	}

	// Level
	[Export]
	public int Level
	{
		get => _level;
		set
		{
			if (_level == value) return;
			_level = Mathf.Max(1, value);
			EmitSignal(SignalName.LevelChanged, _level);
		}
	}

	[Export] public float Speed = 400f;
	
	[Export] public string AnimationSet = "fox";
	[Export] public AudioStream WalkSfx;
	[Export] public PackedScene SlashAttack;
	[Export] private float LungeSpeed = 700f;
	[Export] private float LungeDuration = 0.35f;

	private float currHealth;

	private bool lunging = false;
	private float lungeTime = 0f;
	private Vector2 lungeDir = Vector2.Right;
	
	private AnimatedSprite2D _anim;
	private Sprite2D _sprite;
	private AudioStreamPlayer _audio;
	
	public float GetXpForLevel(int level, float baseXp = 100f, float growth = 1.2f)
	{
		return baseXp * Mathf.Pow(growth, level - 1);
	}
	
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

	private void TakeDamage(int amount, Vector2 dir)
	{
		currHealth -= amount;
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
		currHealth = Health;
		_audio = GetNode<AudioStreamPlayer>("WalkSound");
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_anim.Play(AnimationSet + "_idle");
		Xp = 70;
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput(delta);
		MoveAndSlide();
	}

	private void OnPlayerHurtBoxEntered(Area2D area)
	{
		if (area.IsInGroup("EnemyHitBox"))
		{
			Hitbox hb = (Hitbox)area;
			TakeDamage(hb.Damage, GlobalPosition-hb.GlobalPosition);
			GD.Print(currHealth);
		}
	}
}
