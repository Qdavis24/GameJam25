using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;

public partial class Ally : CharacterBody2D
{
	[Export] public float Speed = 60f;
	
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

	public override void _Process(double delta)
	{
		if (!_isFree) return;
		
		var dir = GetFlowFieldDir();
		Velocity = dir * Speed;
		MoveAndSlide();
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
		var allyCoord = GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer.LocalToMap(
			GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer.ToLocal(GlobalPosition));
		var dir = Vector2.Zero;
		var flowFieldCols = GameManager.Instance.CurrFlowField.Directions.GetLength(0);
		var flowFieldRows = GameManager.Instance.CurrFlowField.Directions.GetLength(1);
		var numSampleDirs = 0;
		for (int colShift = -1; colShift <= 1; colShift++)
		for (int rowShift = -1; rowShift <= 1; rowShift++)
		{
			var currCol = allyCoord.X + colShift;
			var currRow = allyCoord.Y + rowShift;
			if (currCol < 0 || currCol >= flowFieldCols || currRow < 0 || currRow >= flowFieldRows) continue;
			var currDir = GameManager.Instance.CurrFlowField.Directions[currCol, currRow];
			if (currDir == Vector2.Zero) continue;
			dir += currDir;
			numSampleDirs++;
		}

		dir /= numSampleDirs;
		return dir.Normalized();
	}
}
