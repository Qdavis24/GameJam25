using Godot;
using System;
using GameJam25.scripts.world_generation;

public partial class GameManager : Node
{
    [Export] private PackedScene _worldPckdScene;
    [Export] private PackedScene _playerPckdScene;
    [Export] private PackedScene _playerSpawnPckdScene;
    [Export] private Camera _cam;

    public static GameManager Instance;

    public World CurrWorld;
    public FlowField CurrFlowField;
    public Player Player;

    private Vector2I _playerTile;


    public override void _Ready()
    {
        Instance = this;
        InitLevel();
    }

    public override void _PhysicsProcess(double delta)
    {
        var playersPhysicalCoord =
            CurrWorld.PhysicalData.BaseTileMapLayer.LocalToMap(
                CurrWorld.PhysicalData.BaseTileMapLayer.ToGlobal(Player.GlobalPosition)); // big mess but just getting the players tile coord in X,Y
        if ((_playerTile-playersPhysicalCoord).LengthSquared() > 5*5)
        {
            var playersLogicalCoord = (Coord) (playersPhysicalCoord.Y,  playersPhysicalCoord.X);
            _playerTile = playersPhysicalCoord;
            CurrFlowField.GenerateFlowFieldFrom(playersLogicalCoord);
        }
    }

    public void InitLevel()
    {
        SpawnNewWorld();
        SpawnPlayer();
        CurrFlowField = new FlowField(CurrWorld.LogicalData.Matrix, CurrWorld.LogicalData.WalkableState);
        
    }

    private void SpawnNewWorld()
    {
        if (CurrWorld != null) CurrWorld.QueueFree();
        CurrWorld = _worldPckdScene.Instantiate<World>();
        AddChild(CurrWorld);
    }

    private void SpawnPlayer()
    {
        if (Player == null)
        {
            Player = _playerPckdScene.Instantiate<Player>();
            AddChild(Player);
        }

        var spawnPortal = _playerSpawnPckdScene.Instantiate<PlayerSpawn>();
        spawnPortal.GlobalPosition = CurrWorld.LogicalData.PlayerSpawn;
        AddChild(spawnPortal);
        _cam.Target = spawnPortal;
        spawnPortal.PortalOpen += () =>
        {
            Player.GlobalPosition = spawnPortal.GlobalPosition;
            _cam.Target = Player;
        };
    }
}