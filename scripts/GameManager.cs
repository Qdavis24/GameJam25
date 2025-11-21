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
    [Export] private PackedScene _flowFieldScene;
    [Export] private PackedScene _enemyPoolScene;

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
    public EnemyPool EnemyPool;
    public Player Player;
    public Node2D PersistentNodes;

    // Game Manager Logic State
    enum GameState
    {
        MainMenu,
        LoadingGame,
        PlayingGame,
        DeathScreen
    }


    private GameState _gameState = GameState.MainMenu;
    private bool _isPaused;

    private Vector2I _lastPlayerCoord;
    public int NumSpawners;

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

        if (_gameState == GameState.PlayingGame && Input.IsActionJustPressed("respawn"))
        {
            InitWorldLevel();
        }
    }

    private async void InitGame(string character)
    {
        GetTree().Paused = false;
        _isPaused = false;
        
        await _screenFade.FadeToBlack();
        _mainMenu.Close();
        
        XpPool = _xpPoolScene.Instantiate<XpPool>();
        GetTree().Root.AddChild(XpPool);

        EnemyPool = _enemyPoolScene.Instantiate<EnemyPool>();
        GetTree().Root.AddChild(EnemyPool);
        
        PersistentNodes = _persistentNodes.Instantiate<Node2D>();
        GetTree().Root.AddChild(PersistentNodes);
        
        Allies = new() { "fox", "frog", "raccoon", "rabbit" };
        Allies.Remove(character); // take out player character from allie list
        
        Cam = _camScene.Instantiate<Camera>();
        AddChild(Cam);
        
        Player = _playerScene.Instantiate<Player>();
        _ui.InitializeUiFromPlayer(Player); // connects players signals related to stats and upgrade to ui
        _ui.ResetCounters();
        Player.AnimationSet = character;
        Player.Died += OnPlayerDeath;
        PersistentNodes.AddChild(Player);
        
        
        
        InitWorldLevel();
        
        await _screenFade.FadeToNormal();
    }

    private async void EndGame()
    {
        await _screenFade.FadeToBlack();
        EnemyPool.ReturnAllEnemies();
        // cleanup all game components
        Cam.QueueFree();
        Cam = null;
        
        Player.QueueFree();
        Player = null;
        
        RemoveAllies();
        
        World.QueueFree();
        World = null;
        
        FlowField.QueueFree();
        FlowField = null;
        
        XpPool.QueueFree();
        XpPool = null;
        
        EnemyPool.QueueFree();
        EnemyPool = null;
        
        PersistentNodes.QueueFree(); // allies and player are stored in here
        PersistentNodes = null;
        
        // cleanup ui components
        _pauseScreen.Close();
        _deathScreen.Close();
        _mainMenu.Open();
        _gameState = GameState.MainMenu;
        await _screenFade.FadeToNormal();
    }

    public void InitWorldLevel()
    {
        _gameState = GameState.LoadingGame;
        EnemyPool.ReturnAllEnemies();
        Bosses = new() { "rat", "raccoon", "bunny" };
        PauseAllies();
        
        _ui.HideUpgrade(); // prevent upgrade from remaining

        Player.Visible = false;
        Player.InitForWorldLevel();
        
        SpawnWorld();
        
        if (IsInstanceValid(FlowField)) FlowField.QueueFree();
        FlowField = _flowFieldScene.Instantiate<FlowField>();
        GetTree().Root.AddChild(FlowField);
        
        SpawnEnemySpawners();
        
        var enterPortal = SpawnEnterPortal();
        Cam.Target = enterPortal;
        enterPortal.PortalOpen += () =>
        {
            FlowField.Init(World.LogicalData.Matrix, World.LogicalData.NonWalkableState,
                World.PhysicalData.BaseTileMapLayer, Player);
            Player.Visible = true;
            Player.GlobalPosition = enterPortal.GlobalPosition;
            Cam.Target = Player;
            _gameState = GameState.PlayingGame;
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
            if (!ally.IsFreedFromCage)
                continue;
            
            ally.GlobalPosition = Player.GlobalPosition;
            ally.TravellingThroughPortal = false;
        }
    }
    public void RemoveAllies()
    {
        foreach (var ally in AllyInstances.ToArray())
        {
            if (!IsInstanceValid(ally)) 
                continue;
			
            ally.QueueFree();
        }
		
        AllyInstances.Clear();
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
        NumSpawners = 0;
        foreach (Shrine shrine in World.LogicalData.Shrines)
        {
            NumSpawners++;
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
        _ui.IncrementLevelCounter();
        InitWorldLevel();
        await _screenFade.FadeToNormal();
    }

    // Signal Callbacks
    private void OnSpawnerDestroyed(Vector2 pos)
    {
        NumSpawners--;
        if (NumSpawners <= 0)
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
        _deathScreen.Open(_ui.GetCounters());
        _gameState = GameState.DeathScreen;
    }


    public void OpenChest()
    {
        _ui.OpenChest();
    }
    public void EnemyDeathUpdateKillCounter()
    {
        _ui.IncrementKillCounter();
    }
}