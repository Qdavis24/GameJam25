using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;
using GameJam25.scripts.weapons.cloud_weapon;

public partial class CloudWeapon : WeaponBase
{
	[Export] private Area2D _targetingRange;
	[Export] private PackedScene _cloudPackedScene;
	[Export] private Timer _timer;
	[Export] private float _offset = 200.0f;

	private List<Node2D> _targets;

	public override void _Ready()
	{
		_targetingRange.Monitoring = false;
		_timer.Timeout += OnTimeout;
		_targetingRange.BodyEntered += OnTargetingRangeEntered;
		_targetingRange.BodyExited += OnTargetingRangeExited;
		_targets = new List<Node2D>((int)_projCount);
	}
	public override void InitWeapon()
	{
		_targetingRange.Monitoring = false;
		_targets = new List<Node2D>((int)_projCount);
		_targetingRange.Monitoring = true;
		_active = true;
		_timer.WaitTime = _projCooldown;
		_timer.Start();
	}

	private void SpawnCloud(Node2D target)
	{
		var cloud = _cloudPackedScene.Instantiate<Cloud>();
		cloud.Damage = _projDamage;
		cloud.Speed = _projSpeed;
		cloud.Target = target;
		cloud.GlobalPosition = GlobalPosition + new Vector2(
			_offset * (GD.Randf() * 2 - 1), 
			_offset * (GD.Randf() * 2 - 1)
		);
		GameManager.Instance.World.AddChild(cloud);
	}

	public void OnTimeout()
	{
		int cloudsToSpawn = Math.Min((int)_projCount, _targets.Count);
		for (int i = 0; i < cloudsToSpawn; i++)
		{
			SpawnCloud(_targets[i]);
		}
		_targets.RemoveRange(0, cloudsToSpawn);
	}

	public void OnTargetingRangeEntered(Node2D body)
	{
		if (!body.IsInGroup("Enemies")) return;
		_targets.Add(body);
	}
	
	public void OnTargetingRangeExited(Node2D body)
	{
		if (!body.IsInGroup("Enemies")) return;
		_targets.Remove(body);
	}
}
