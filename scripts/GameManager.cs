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
    [Export] private CanvasModulate _canvasModulate;
    [Export] private Ui _ui;
    
    // Game state
    public XpPool XpPool;
    public World World;
    public FlowField FlowField;
    public Player Player;
    
    // Game Manager Logic State
    enum GameState {
        StartMenu,
        LoadingGame,
        PlayingGame,
        DeathScreen
    }
    private bool _isPaused = false;
    
    private GameState _gameState;
    private Vector2I _lastPlayerCoord;
    private int _numSpawners;
    
    public static GameManager Instance;

    public override void _Ready()
    {
        Instance = this;
        XpPool = new XpPool(_xpOrbScene, 100, World);
    }


    public override void _PhysicsProcess(double delta)
    {
        switch (_gameState)
        {
            case GameState.StartMenu:
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

    private void InitGame()
    {
        Player = _playerScene.Instantiate<Player>();
        _ui.InitializeUiFromPlayer(Player);
        World = _worldScene.Instantiate<World>();
        FlowField = new FlowField(World.LogicalData.Matrix, World.LogicalData.NonWalkableState);
    }

    public async void InitWorldLevel()
    {
        _canvasModulate.LevelTransition();
        Player.InitForWorldLevel();
        SpawnWorld();
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

    private void SpawnWorld()
    {
        if (World != null) World.QueueFree();
        World = _worldScene.Instantiate<World>();
        AddChild(World);
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
    
}