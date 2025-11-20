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

	[ExportCategory("Scene References")] 
	[Export] private PackedScene _xpOrbScene;
	[Export] private PackedScene _worldScene;
	[Export] private PackedScene _playerScene;
	[Export] private PackedScene _enterPortalScene;
	[Export] private PackedScene _exitPortalScene;
	[Export] private PackedScene _enemySpawnerScene;
	[Export] private PackedScene _camScene;
	[Export] private PackedScene _xpPoolScene;
	[Export] private PackedScene _persistentNodes;
	
	[ExportCategory("Node References")]
	[Export] private ScreenFade _screenFade;
	// ui below
	[Export] private Ui _ui;
	[Export] private MainMenu _mainMenu;
	[Export] private PauseScreen _pauseScreen;
	[Export] private DeathScreen _deathScreen;

	public List<string> Bosses;
	public List<string> Allies;
	public List<Ally> AllyInstances = new();
	
	// Game state
	public Camera Cam;
	public XpPool XpPool;
	public World World;
	public FlowField FlowField;
	public Player Player;
	public Node2D PersistentNodes;

	// Game Manager Logic State
	enum GameState {
		MainMenu,
		LoadingGame,
		PlayingGame,
		DeathScreen
	}
	
	
	private GameState _gameState = GameState.MainMenu;
	private bool _isPaused;
	
	private Vector2I _lastPlayerCoord;
	private int _numSpawners;
	
	public static GameManager Instance;
	
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Instance = this;
		XpPool = _xpPoolScene.Instantiate<XpPool>();
		GetTree().Root.CallDeferred("add_child", XpPool);
		PersistentNodes = _persistentNodes.Instantiate<Node2D>();
		GetTree().Root.CallDeferred("add_child", PersistentNodes);
		_mainMenu.Show();
		
		_mainMenu.InitGame += InitGame;
		_pauseScreen.MainMenuRequested += EndGame;
		_pauseScreen.Unpause += () =>
		{
			_pauseScreen.Close();
			GetTree().Paused = false;
			_isPaused = false;
		};
		_deathScreen.MainMenuRequested += EndGame;
		
	}

	public override void _PhysicsProcess(double delta)
	{
		switch (_gameState)
		{
			case GameState.MainMenu:
				break;
			case GameState.LoadingGame:
				break;
			case GameState.PlayingGame:
				if (!_isPaused)
					UpdateFlowField();
				if (Input.IsActionJustPressed("respawn"))
				{
					ChangeLevel();
				}
				break;
			case GameState.DeathScreen:
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (_gameState == GameState.PlayingGame && Input.IsActionJustPressed("pause"))
		{
			if (_isPaused)
			{
				_pauseScreen.Close();
				GetTree().Paused = false;
			}
			else
			{
				_pauseScreen.Open();
				GetTree().Paused = true;
			}
			_isPaused = !_isPaused;
		}
	}

	private async void InitGame(string character)
	{
		Allies = new() { "fox", "frog", "raccoon", "rabbit" };
		Allies.Remove(character); // take out player character from ally list
		GetTree().Paused = false;
		_isPaused = false;
		Cam = _camScene.Instantiate<Camera>();
		AddChild(Cam);
		await _screenFade.FadeToBlack();
		_mainMenu.Close();
		Player = _playerScene.Instantiate<Player>();
		Player.AnimationSet = character;
		_ui.InitializeUiFromPlayer(Player); // connects players signals related to stats and upgrade to ui
		PersistentNodes.AddChild(Player);
		Player.Died += OnPlayerDeath;
		InitWorldLevel();
		await _screenFade.FadeToNormal();
	}

	private async void EndGame()
	{
		await _screenFade.FadeToBlack();
		// cleanup all game components
		Cam.QueueFree();
		Cam = null;
		Player.QueueFree();
		Player = null;
		World.QueueFree();
		World = null;
		FlowField = null;
		// cleanup ui components
		_pauseScreen.Close();
		_deathScreen.Close();
		_mainMenu.Open();
		_gameState = GameState.MainMenu;
		await _screenFade.FadeToNormal();
	}

	public void InitWorldLevel()
	{
		Bosses = new() { "rat", "raccoon", "bunny"};
		PauseAllies();
		
		Player.Visible = false;
		_gameState = GameState.LoadingGame;
		Player.InitForWorldLevel();
		SpawnWorld();
		FlowField = new FlowField(World.LogicalData.Matrix,
			World.LogicalData.NonWalkableState);
		SpawnEnemySpawners();
		var enterPortal = SpawnEnterPortal();
		Cam.Target = enterPortal;
		enterPortal.PortalOpen += () =>
		{
			Player.Visible = true;
			Player.GlobalPosition = enterPortal.GlobalPosition;
			Cam.Target = Player;
			_gameState = GameState.PlayingGame;
			_lastPlayerCoord = GetPlayerCoord();
			ResetAllies();
		};
		ScaleDifficulty();
	}
	
	public void PauseAllies()
	{
		foreach (var ally in AllyInstances.ToArray())
		{
			if (!IsInstanceValid(ally)) 
				continue;
			// Mark as travelling so their own logic can behave differently
			ally.TravellingThroughPortal = true;
		}
	}
	
	public void ResetAllies()
	{
		foreach (var ally in AllyInstances.ToArray())
		{
			if (!IsInstanceValid(ally)) 
				continue;
			
			ally.GlobalPosition = Player.GlobalPosition;
			ally.TravellingThroughPortal = false;
		}
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
		foreach (Shrine shrine in World.LogicalData.Shrines)
		{
			_numSpawners++;
			var newSpawner = _enemySpawnerScene.Instantiate<EnemySpawner>();
			newSpawner.Init(_enemySpawnerHealth, _enemySpawnerWaveInterval, _enemySpawnerNumEnemiesPerWave,
				_enemySpawnerNumWaves);
			newSpawner.GlobalPosition = World.PhysicalData.BaseTileMapLayer.MapToLocal(shrine.CenterTile);
			newSpawner.SpawnerDestroyed += OnSpawnerDestroyed;
			World.CallDeferred("add_child", newSpawner);
		}
	}

	private void SpawnWorld()
	{
		if (World != null)
		{
			World.QueueFree();
		}
		World = _worldScene.Instantiate<World>();
		
		GetTree().Root.AddChild(World);
	}

	private EnterPortal SpawnEnterPortal()
	{
		var spawnPortal = _enterPortalScene.Instantiate<EnterPortal>();
		spawnPortal.GlobalPosition = World.LogicalData.PlayerSpawn;
		AddChild(spawnPortal);
		return spawnPortal;
	}

	// Helper Methods
	private async void ChangeLevel()
	{
		await _screenFade.FadeToBlack();
		InitWorldLevel();
		await _screenFade.FadeToNormal();
	}
	private void UpdateFlowField()
	{
		var currPlayerCoord = GetPlayerCoord();
		if ((_lastPlayerCoord - currPlayerCoord).LengthSquared() > 5 * 5)
		{
			FlowField.GenerateFlowFieldFrom(currPlayerCoord);
			_lastPlayerCoord = currPlayerCoord;
		}
	}
	private Vector2I GetPlayerCoord()
	{
		return World.PhysicalData.BaseTileMapLayer.LocalToMap(
			World.PhysicalData.BaseTileMapLayer
				.ToLocal(Player.GlobalPosition)); // big mess but just getting the players tile coord
	}
	
	// Signal Callbacks
	private void OnSpawnerDestroyed(Vector2 pos)
	{
		_numSpawners--;
		if (_numSpawners <= 0)
		{
			var exitPortal = _exitPortalScene.Instantiate<ExitPortal>();
			exitPortal.GlobalPosition = pos;
			exitPortal.PlayerEnteredPortal += ChangeLevel;
			World.CallDeferred("add_child", exitPortal);
		}
	}

	private void OnPlayerDeath()
	{
		GetTree().Paused = true;
		_isPaused = true;
		Cam.DeathAnim();
		_deathScreen.Open();
		_gameState = GameState.DeathScreen;
	}


	public void OpenChest()
	{
		_ui.OpenChest();
	}
}
