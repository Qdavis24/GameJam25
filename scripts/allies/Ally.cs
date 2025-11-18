using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;

public partial class Ally : Node2D
{
	[Export] public PackedScene FoxWeaponScene;
	[Export] public PackedScene FrogWeaponScene;
	[Export] public PackedScene RaccoonWeaponScene;
	//[Export] public PackedScene RabbitWeaponScene;
	private Dictionary<string, PackedScene> _weapons;

	public string Species; // set in EnemySpawner scene
	
	private AnimatedSprite2D _anim;
	private Sprite2D _cage;
	
	public override void _Ready()
	{
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
	}

	public void FreeFromCage()
	{
		_cage.QueueFree();
		// hook up to flow state
		_anim.Play(Species + "_walk");
		
		var weapon = _weapons[Species].Instantiate<WeaponBase>();
		AddChild(weapon);
		weapon.InitWeapon();
		
	}
}
