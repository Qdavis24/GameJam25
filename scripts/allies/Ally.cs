using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;

public partial class Ally : CharacterBody2D
{
	[Export] public float Speed = 60f;

	// Boids settings
	[Export] public float BoidsRadius = 120f;          // how far they "see" other allies
	[Export] public float SeparationWeight = 1.5f;     // push apart
	[Export] public float AlignmentWeight = 0.4f;      // match velocity
	[Export] public float CohesionWeight = 0.3f;       // move toward group center
	[Export] public float BoidsInfluence = 0.7f;       // how much boids affect flow direction

	// Stopping / jitter control
	[Export] public float StopVelocityEpsilon = 5f;    // if slower than this -> treat as stopped
	[Export] public float SteeringDeadzone = 0.01f;    // ignore tiny steering
	[Export] public float DampingFactor = 8f;          // how quickly they slow when no steering

	[Export] public PackedScene FoxWeaponScene;
	[Export] public PackedScene FrogWeaponScene;
	[Export] public PackedScene RaccoonWeaponScene;
	[Export] public PackedScene RabbitWeaponScene;
	private Dictionary<string, PackedScene> _weapons;

	public string Species; // set in EnemySpawner scene
	public bool TravellingThroughPortal;

	private AnimatedSprite2D _anim;
	private Sprite2D _cage;

	private bool _isFree;
	
	// flipH timer stuff to prevent jittering
	private float _flipCooldown = 0.15f; // seconds between allowed flips
	private float _flipTimer = 0f;
	private int _facing = 1; // 1 = right, -1 = left

	public override void _Ready()
	{
		AddToGroup("allies");
		_isFree = false;

		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_cage = GetNode<Sprite2D>("Cage");

		_anim.Play(Species + "_idle");

		_weapons = new Dictionary<string, PackedScene>()
		{
			{ "fox", FoxWeaponScene },
			{ "frog", FrogWeaponScene },
			{ "raccoon", RaccoonWeaponScene },
			{"rabbit", RabbitWeaponScene}
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isFree || TravellingThroughPortal || (GameManager.Instance.FlowField.Directions == null))
			return;

		// 1) Base direction from flow field
		Vector2 flowDir = GetFlowFieldDir();

		// 2) Steering direction we’ll build
		Vector2 steering = Vector2.Zero;

		if (flowDir.LengthSquared() > 0.0001f)
		{
			// Only do boids if we actually have somewhere to go
			Vector2 boidsDir = GetBoidsDir();

			if (boidsDir != Vector2.Zero)
			{
				// Blend flow + boids
				steering = (flowDir + boidsDir * BoidsInfluence).Normalized();
			}
			else
			{
				steering = flowDir;
			}
		}
		else
		{
			// Flow field wants us to stop; no separation at rest.
			steering = Vector2.Zero;
		}

		// 3) Apply steering with deadzone + damping to kill jitter
		if (steering.LengthSquared() > SteeringDeadzone)
		{
			Velocity = steering * Speed;
		}
		else
		{
			// Gradually slow down instead of instant flip/flop
			Velocity = Velocity.MoveToward(Vector2.Zero, (float)(DampingFactor * delta * Speed));

			if (Velocity.LengthSquared() < StopVelocityEpsilon * StopVelocityEpsilon)
				Velocity = Vector2.Zero;
		}

		MoveAndSlide();

		// 4) Animations + flipping
		if (Velocity.LengthSquared() > StopVelocityEpsilon * StopVelocityEpsilon)
		{
			_flipTimer -= (float)delta;

			if (Mathf.Abs(Velocity.X) > 1f)
			{
				int desiredFacing = Velocity.X < 0 ? -1 : 1;

				// Only try to flip if:
				// - the desired direction changed, and
				// - our cooldown has finished
				if (desiredFacing != _facing && _flipTimer <= 0f)
				{
					_facing = desiredFacing;
					_anim.FlipH = _facing < 0;
					_flipTimer = _flipCooldown;
				}
			}

			_anim.Play(Species + "_walk");
		}
		else
		{
			_anim.Play(Species + "_idle");
		}
	}

	public void FreeFromCage()
	{
		var allyInstances = GameManager.Instance.AllyInstances;
		allyInstances.Add(this);

		_isFree = true;
		_anim.Play(Species + "_walk");

		// Free the cage AFTER physics step
		if (IsInstanceValid(_cage))
			_cage.CallDeferred(Node.MethodName.QueueFree);

		// Spawn and init the weapon AFTER physics step
		CallDeferred(MethodName.SpawnWeapon);
	}

	private void SpawnWeapon()
	{
		var weapon = _weapons[Species].Instantiate<WeaponBase>();
		AddChild(weapon);
		weapon.InitWeapon();
	}

	// ---------- FLOW FIELD ----------

	private Vector2 GetFlowFieldDir()
	{
		var tilemap = GameManager.Instance.World.PhysicalData.BaseTileMapLayer;
		var allyCoord = tilemap.LocalToMap(tilemap.ToLocal(GlobalPosition));

		var dir = Vector2.Zero;
		int flowFieldCols = GameManager.Instance.FlowField.Directions.GetLength(0);
		int flowFieldRows = GameManager.Instance.FlowField.Directions.GetLength(1);
		int numSampleDirs = 0;

		for (int colShift = -1; colShift <= 1; colShift++)
		for (int rowShift = -1; rowShift <= 1; rowShift++)
		{
			int currCol = allyCoord.X + colShift;
			int currRow = allyCoord.Y + rowShift;
			if (currCol < 0 || currCol >= flowFieldCols || currRow < 0 || currRow >= flowFieldRows)
				continue;

			var currDir = GameManager.Instance.FlowField.Directions[currCol, currRow];
			if (currDir == Vector2.Zero)
				continue;

			dir += currDir;
			numSampleDirs++;
		}

		if (numSampleDirs == 0)
			return Vector2.Zero;

		dir /= numSampleDirs;

		if (dir.LengthSquared() < 0.0001f)
			return Vector2.Zero;

		return dir.Normalized();
	}

	// ---------- BOIDS STEERING ----------

	private Vector2 GetBoidsDir()
	{
		var separationVec = Vector2.Zero;
		var alignmentVec = Vector2.Zero;
		var cohesionPos = Vector2.Zero;

		int count = 0;

		foreach (Node node in GetTree().GetNodesInGroup("allies"))
		{
			if (node == this)
				continue;

			if (node is not Ally other)
				continue;

			Vector2 toOther = other.GlobalPosition - GlobalPosition;
			float dist = toOther.Length();
			if (dist <= 0f || dist > BoidsRadius)
				continue;

			count++;

			// Separation: push away from neighbors
			separationVec -= toOther.Normalized(); // from other to me

			// Alignment: match velocity
			alignmentVec += other.Velocity;

			// Cohesion: move toward center of mass
			cohesionPos += other.GlobalPosition;
		}

		if (count == 0)
			return Vector2.Zero;

		alignmentVec /= count;
		cohesionPos /= count;

		Vector2 cohesionVec = (cohesionPos - GlobalPosition);
		if (cohesionVec.LengthSquared() > 0.0001f)
			cohesionVec = cohesionVec.Normalized();
		else
			cohesionVec = Vector2.Zero;

		if (alignmentVec.LengthSquared() > 0.0001f)
			alignmentVec = alignmentVec.Normalized();
		else
			alignmentVec = Vector2.Zero;

		if (separationVec.LengthSquared() > 0.0001f)
			separationVec = separationVec.Normalized();
		else
			separationVec = Vector2.Zero;

		Vector2 boids =
			separationVec * SeparationWeight +
			alignmentVec * AlignmentWeight +
			cohesionVec * CohesionWeight;

		if (boids.LengthSquared() < 0.0001f)
			return Vector2.Zero;

		return boids.Normalized();
	}
}
