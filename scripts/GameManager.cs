using Godot;
using System;
using GameJam25.scripts.world_generation;
public partial class GameManager : Node
{
	[Export] private PackedScene _worldPckdScene;
	[Export] private PackedScene _playerPckdScene;
	[Export] private PackedScene _playerSpawnPckdScene;

	[Export] private PlayerCamera _cam;

	private World _currWorld;
	private Player _currPlayer;

	public override void _Ready()
	{
		_currPlayer = _playerPckdScene.Instantiate<Player>();
		AddChild(_currPlayer);
		GetNode<Ui>("Ui").SetPlayer(_currPlayer);
		SpawnNewWorld();
		SpawnPlayer();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("regen"))
		{
			SpawnNewWorld();
			SpawnPlayer();
		}
	}

	private void SpawnNewWorld()
	{
		if (_currWorld != null) _currWorld.QueueFree();
		_currWorld = _worldPckdScene.Instantiate<World>();
		AddChild(_currWorld);
	}

	private void SpawnPlayer()
	{
	 
		var spawnPortal = _playerSpawnPckdScene.Instantiate<PlayerSpawn>();
		spawnPortal.GlobalPosition = _currWorld.LogicalWorldData.PlayerSpawn;
		AddChild(spawnPortal);
		_cam.Target = spawnPortal;
		GD.Print(_cam.Target.GlobalPosition, _cam.GlobalPosition);   
		spawnPortal.PortalOpen += () => { _currPlayer.GlobalPosition = spawnPortal.GlobalPosition; _cam.Target = _currPlayer;};

	}
}
