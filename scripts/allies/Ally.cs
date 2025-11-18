using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;

public partial class Ally : CharacterBody2D
{
	[Export] public float Speed = 60f;
	
	[Export] public float SeparationRadius = 200f;      // how far they care about others
	[Export] public float SeparationStrength = 300f;   // how strongly they push away

	
	[Export] public PackedScene FoxWeaponScene;
	[Export] public PackedScene FrogWeaponScene;
	[Export] public PackedScene RaccoonWeaponScene;
	//[Export] public PackedScene RabbitWeaponScene;
	private Dictionary<string, PackedScene> _weapons;

	public string Species; // set in EnemySpawner scene
	
	private AnimatedSprite2D _anim;
	private Sprite2D _cage;
	
	private bool _isFree;
	
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
			{ "raccoon", RaccoonWeaponScene }
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isFree) return; // your existing flag

		var dir = GetFlowFieldDir();          // existing function (don’t change it)
		var separation = GetSeparation();     // new

		// Combine: flow direction + separation steering
		Vector2 finalDir = dir + separation * (SeparationStrength / Speed);
		if (finalDir.LengthSquared() > 0.001f)
			finalDir = finalDir.Normalized();

		Velocity = finalDir * Speed;
		MoveAndSlide();

		// Flip + idle when basically not moving
		if (Velocity.LengthSquared() > 2f)
		{
			if (Velocity.X != 0)
				_anim.FlipH = Velocity.X < 0;

			_anim.Play(Species + "_walk");
		}
		else
		{
			_anim.Play(Species + "_idle");
		}
	}

	public void FreeFromCage()
	{
		_isFree = true;
		_cage.QueueFree();
		// hook up to flow state
		_anim.Play(Species + "_walk");
		
		var weapon = _weapons[Species].Instantiate<WeaponBase>();
		AddChild(weapon);
		weapon.InitWeapon();
		
	}
	
	private Vector2 GetFlowFieldDir()
	{
		var tilemap = GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer;
		var allyCoord = tilemap.LocalToMap(tilemap.ToLocal(GlobalPosition));
		
		var dir = Vector2.Zero;
		int flowFieldCols = GameManager.Instance.CurrFlowField.Directions.GetLength(0);
		int flowFieldRows = GameManager.Instance.CurrFlowField.Directions.GetLength(1);
		int numSampleDirs = 0;

		for (int colShift = -1; colShift <= 1; colShift++)
		for (int rowShift = -1; rowShift <= 1; rowShift++)
		{
			int currCol = allyCoord.X + colShift;
			int currRow = allyCoord.Y + rowShift;
			if (currCol < 0 || currCol >= flowFieldCols || currRow < 0 || currRow >= flowFieldRows)
				continue;

			var currDir = GameManager.Instance.CurrFlowField.Directions[currCol, currRow];
			if (currDir == Vector2.Zero)
				continue;

			dir += currDir;
			numSampleDirs++;
		}

		if (numSampleDirs == 0)
			return Vector2.Zero;

		dir /= numSampleDirs;

		// If direction is super small, treat it as “no movement”
		if (dir.LengthSquared() < 0.0001f)
			return Vector2.Zero;

		return dir.Normalized();
	}
	
	private Vector2 GetSeparation()
	{
		var separation = Vector2.Zero;
		int count = 0;

		foreach (Node node in GetTree().GetNodesInGroup("allies"))
		{
			if (node == this) continue;

			var other = node as Ally;
			if (other == null) continue;

			Vector2 toMe = GlobalPosition - other.GlobalPosition;
			float dist = toMe.Length();
			if (dist <= 0f || dist > SeparationRadius) continue;

			// Stronger push when closer
			float weight = (SeparationRadius - dist) / SeparationRadius;
			separation += toMe.Normalized() * weight;
			count++;
		}

		if (count > 0)
			separation /= count;

		return separation;
	}
}
