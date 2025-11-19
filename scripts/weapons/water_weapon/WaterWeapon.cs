using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using GameJam25.scripts.weapons.base_classes;
using GameJam25.scripts.weapons.water_weapon;
using Timer = Godot.Timer;

public partial class WaterWeapon : WeaponBase
{
	[Export] private PackedScene _waterPackedScene;

	private Timer _timer;

	private List<Water> _waters;
	private float[] _dropAngles;
	private float _offset = 80.0f;


	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += OnTimeout;
		_waters = new();
	}


	public override void InitWeapon()
	{
		foreach (Water water in _waters) water.QueueFree();
		_waters = new ();
		_dropAngles = new float[(int)_projCount];
		GetStartingAngles();
		_timer.WaitTime = _projCooldown;
		_timer.Start();
		CallDeferred(MethodName.OnTimeout);
	}

	private void GetStartingAngles()
	{
		var angleBetweenDrops = Mathf.Tau / (int)_projCount;
		var currRad = 0.0f;
		for (int i = 0; i < (int)_projCount; i++)
		{
			_dropAngles[i] = currRad;
			currRad += angleBetweenDrops;
		}
	}

	public void OnTimeout()
	{
		var fireIntervalTime = (_projCooldown - 1.0) / (int)_projCount;
		for (int i = 0; i < (int)_projCount; i++)
		{
			var spawnDir = Vector2.FromAngle(_dropAngles[i]);
			var newDrop = _waterPackedScene.Instantiate<Water>();
			newDrop.Position += spawnDir * _offset;
			AddChild(newDrop);
			newDrop.Sprite.Rotation = _dropAngles[i];
			newDrop.Timer.WaitTime = fireIntervalTime * (i + 1);
			newDrop.Timer.Start();
			newDrop.Damage = _projDamage;
			newDrop.Scale *= _projSize;
			newDrop.Speed = _projSpeed;
			newDrop.TreeExited += () => _waters.Remove(newDrop);
			_waters.Add(newDrop);
		}
	}
}
