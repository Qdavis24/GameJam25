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
    
    [ExportCategory("Node References")]
    [Export] public Camera Cam;
    [Export] private ScreenFade _screenFade;
    // ui below
    [Export] private Ui _ui;
    [Export] private MainMenu _mainMenu;
    [Export] private PauseScreen _pauseScreen;
    [Export] private DeathScreen _deathScreen;
    
    // Game state
    public XpPool XpPool;
    public World World;
    public FlowField FlowField;
    public Player Player;
    
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
        await _screenFade.FadeToBlack();
        _mainMenu.Close();
        
        Player = _playerScene.Instantiate<Player>();
        Player.AnimationSet = character;
        _ui.InitializeUiFromPlayer(Player); // connects players signals related to stats and upgrade to ui
        Player.Died += OnPlayerDeath; 
        InitWorldLevel();

        await _screenFade.FadeToNormal();
        _gameState = GameState.PlayingGame;

    }

    private async void EndGame()
    {
        await _screenFade.FadeToBlack();
        // cleanup all game components
        Cam.Target = null;
        Cam.Reset();
        Player.QueueFree();
        Player = null;
        World.QueueFree();
        World = null;
        XpPool.Cleanup();
        XpPool = null;
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
        Player.InitForWorldLevel();
        GetTree().Root.AddChild(Player);
        World = SpawnWorld();
        GetTree().Root.AddChild(World);
        XpPool = new XpPool(_xpOrbScene, 100, World);
        SpawnEnemySpawners();
        var enterPortal = SpawnEnterPortal();
        Cam.Target = enterPortal;
        enterPortal.PortalOpen += () =>
        {
            Player.GlobalPosition = enterPortal.GlobalPosition;
            Player.Velocity = new Vector2(100, 20);
            Cam.Target = Player;
        };
        FlowField = new FlowField(World.LogicalData.Matrix,
            World.LogicalData.NonWalkableState);
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

    private World SpawnWorld()
    {
        if (World != null) World.QueueFree();
        return _worldScene.Instantiate<World>();
    }

    private EnterPortal SpawnEnterPortal()
    {
        var spawnPortal = _enterPortalScene.Instantiate<EnterPortal>();
        spawnPortal.GlobalPosition = World.LogicalData.PlayerSpawn;
        AddChild(spawnPortal);
        return spawnPortal;
    }

    // Helper Methods
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
        GD.Print("spawner destroyed");
        _numSpawners--;
        if (_numSpawners <= 0)
        {
            var exitPortal = _exitPortalScene.Instantiate<ExitPortal>();
            exitPortal.GlobalPosition = pos;
            exitPortal.PlayerEnteredPortal += InitWorldLevel;
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
    
}