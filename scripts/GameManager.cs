using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts;
using GameJam25.scripts.world_generation;

public partial class GameManager : Node
{
	[ExportCategory("Starting Difficulty")] 
	[Export] private float _enemySpawnerHealth = 100.0f;
	[Export] private float _enemySpawnerNumEnemiesPerWave = 5.0f;
	[Export] private float _enemySpawnerWaveInterval = 45.0f;
	[Export] private float _enemySpawnerNumWaves = 3.0f;
	
	[ExportCategory("Difficulty Ramps")]
	[Export] private float _enemySpawnerHealthScalr = 1.1f;
	[Export] private float _enemySpawnerNumEnemiesPerWaveScalr = 1.1f;
	[Export] private float _enemySpawnerWaveIntervalScalr = .9f;
	[Export] private float _enemySpawnerNumWavesScalr = 1.1f;

	[ExportCategory("External Required Resources")] 
	[Export] private PackedScene _xpOrbPckdScene;
	[Export] private PackedScene _worldPckdScene;
	[Export] private PackedScene _playerPckdScene;
	[Export] private PackedScene _playerSpawnPckdScene;
	[Export] private PackedScene _enemySpawnerPckdScene;
	[Export] private PackedScene _arrowPckdScene;
	[Export] private PackedScene _labelPckdScene;
	
	// Internal Required Resources
	private Ui _ui;
	public Camera Cam;


	// State Instances
	public XpPool XpPool;
	public World CurrWorld;
	public FlowField CurrFlowField;
	public Player Player;
	private Vector2I _lastPlayerCoord;
	private int _numSpawners;
	private List<Node2D> _arrows = new();
	

	public static GameManager Instance;
	
	public override void _Ready()
	{
		_ui = GetNode<Ui>("Ui");
		Cam = GetNode<Camera>("Camera2D");
		
		Instance = this;
	}
	
	public void StartNewGame(string character)
	{
		Player = _playerPckdScene.Instantiate<Player>();
		Player.AnimationSet = character;
		_ui.InitializeUiFromPlayer(Player);
		AddChild(Player);
		InitLevel();
		XpPool = new XpPool(_xpOrbPckdScene, 100, CurrWorld);
	}


	public override void _PhysicsProcess(double delta)
	{
		UpdateFlowField(false);
	}
	
	private void UpdateFlowField(bool debug)
	{
		var currPlayerCoord =
			CurrWorld.PhysicalData.BaseTileMapLayer.LocalToMap(
				CurrWorld.PhysicalData.BaseTileMapLayer
					.ToLocal(Player.GlobalPosition)); // big mess but just getting the players tile coord
		if ((_lastPlayerCoord - currPlayerCoord).LengthSquared() > 5 * 5)
		{
			CurrFlowField.GenerateFlowFieldFrom(currPlayerCoord);
			_lastPlayerCoord = currPlayerCoord;
			if (debug)
			{
				if (_arrows.Count > 0)
					foreach (var ar in _arrows)
						ar.QueueFree();
				for (int col = 0; col < CurrFlowField.Directions.GetLength(0); col++)
				for (int row = 0; row < CurrFlowField.Directions.GetLength(1); row++)
				{
					CreateLabel(col, row, $"{CurrFlowField.Costs[col, row]}");
					CreateArrow(col, row, CurrFlowField.Directions[col, row]);
				}
				
			}
		}
	}

	public void InitLevel()
	{
		SpawnNewWorld();
		SpawnPlayer();
		CurrFlowField = new FlowField(CurrWorld.LogicalData.Matrix,
			CurrWorld.LogicalData.NonWalkableState);
		SpawnEnemySpawners();
		ScaleDifficulty();
	}

	private void ScaleDifficulty()
	{
		_enemySpawnerHealth *= _enemySpawnerHealthScalr;
		_enemySpawnerWaveInterval *= _enemySpawnerWaveIntervalScalr;
		_enemySpawnerNumEnemiesPerWave *= _enemySpawnerNumEnemiesPerWaveScalr;
		_enemySpawnerNumWaves *= _enemySpawnerNumWavesScalr;
	}


	private void SpawnEnemySpawners()
	{
		_numSpawners = 0;
		foreach (Shrine shrine in CurrWorld.LogicalData.Shrines)
		{
			_numSpawners++;
			var newSpawner = _enemySpawnerPckdScene.Instantiate<EnemySpawner>();
			newSpawner.Init(_enemySpawnerHealth, _enemySpawnerWaveInterval, _enemySpawnerNumEnemiesPerWave,
				_enemySpawnerNumWaves);
			newSpawner.GlobalPosition = CurrWorld.PhysicalData.BaseTileMapLayer.MapToLocal(shrine.CenterTile);
			newSpawner.SpawnerDestroyed += OnSpawnerDestroyed;
			CurrWorld.AddChild(newSpawner);
		}
	}

	private void SpawnNewWorld()
	{
		if (CurrWorld != null) CurrWorld.QueueFree();
		CurrWorld = _worldPckdScene.Instantiate<World>();
		AddChild(CurrWorld);
	}

	private void SpawnPlayer()
	{
		var spawnPortal = _playerSpawnPckdScene.Instantiate<PlayerSpawn>();
		spawnPortal.GlobalPosition = CurrWorld.LogicalData.PlayerSpawn;
		AddChild(spawnPortal);
		Cam.Target = spawnPortal;
		spawnPortal.PortalOpen += () =>
		{
			Player.GlobalPosition = spawnPortal.GlobalPosition;
			Cam.Target = Player;
		};
	}
	
	private void OnSpawnerDestroyed()
	{
		_numSpawners--;
		if (_numSpawners <= 0)
		{
			GD.Print("Level Complete");
		}
	}
	// DEBUG STUFF BELOW
	private void CreateArrow(int col, int row, Vector2 direction)
	{
		var arrow = _arrowPckdScene.Instantiate<Polygon2D>();
		var tilePos =
			CurrWorld.PhysicalData.BaseTileMapLayer.ToGlobal(
				CurrWorld.PhysicalData.BaseTileMapLayer.MapToLocal(new Vector2I(col, row)));
		var nextTilePos =
			CurrWorld.PhysicalData.BaseTileMapLayer.ToGlobal(
				CurrWorld.PhysicalData.BaseTileMapLayer.MapToLocal(new Vector2I(col + (int)direction.X, row + (int)direction.Y)));
		arrow.GlobalPosition = tilePos;
		AddChild(arrow);
		arrow.Rotation = (direction).Angle();
		_arrows.Add(arrow);
	}

	private void CreateLabel(int col, int row, string cost)
	{
		var l = _labelPckdScene.Instantiate<Label>();
		l.Text = cost;
		l.Position = CurrWorld.PhysicalData.BaseTileMapLayer.MapToLocal(new Vector2I(col, row));
		AddChild(l);
	}

}
