using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts;

public partial class XpPool : Node2D
{
	[Export] private float _spawnOffset = 10f;
	[Export] int _poolSize = 100;
	[Export] private PackedScene _xpOrbPckdScene;
	private Queue<Xp> _orbPool;

	public override void _Ready()
	{
		_orbPool = new Queue<Xp>(_poolSize);
		for (int i = 0; i < _poolSize; i++)
		{
			var newOrb = _xpOrbPckdScene.Instantiate<Xp>();
			AddChild(newOrb);
			_orbPool.Enqueue(newOrb);
		}
	}

	public async void SpawnXpAt(int amount, Vector2 globalPosition)
	{
		var radIncr = Mathf.Tau / amount;
		for (int i = 0; i < amount; i++)
		{
			Xp newOrb;
			if (_orbPool.Count == 0)
				newOrb = _xpOrbPckdScene.Instantiate<Xp>();
			else
				newOrb = _orbPool.Dequeue();
			
			newOrb.Enable();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			newOrb.GlobalPosition = globalPosition + Vector2.FromAngle(radIncr * i) * _spawnOffset;
		}
	}

	public void ReturnXp(Xp xp)
	{
		xp.Disable();
		_orbPool.Enqueue(xp);
	}
	
}
