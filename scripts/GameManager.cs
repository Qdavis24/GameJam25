using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.world_generation;

public partial class GameManager : Node
{
    [Export] private PackedScene _worldPckdScene;
    [Export] private PackedScene _playerPckdScene;
    [Export] private PackedScene _playerSpawnPckdScene;
    [Export] private Camera _cam;
    [Export] private PackedScene _arrow;
    [Export] private PackedScene _label;

    public static GameManager Instance;

    public World CurrWorld;
    public FlowField CurrFlowField;
    public Player Player;

    private Vector2I _lastPlayerCoord;

    private List<Node2D> _arrows = new();

    public override void _Ready()
    {
        Instance = this;
        InitLevel();
    }

    private void CreateArrow(int col, int row, Vector2 direction)
    {
        var arrow = _arrow.Instantiate<Polygon2D>();
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
        var l = _label.Instantiate<Label>();
        l.Text = cost;
        l.Position = CurrWorld.PhysicalData.BaseTileMapLayer.MapToLocal(new Vector2I(col, row));
        AddChild(l);
    }

    public override void _PhysicsProcess(double delta)
    {
        var currPlayerCoord =
            CurrWorld.PhysicalData.BaseTileMapLayer.LocalToMap(
                CurrWorld.PhysicalData.BaseTileMapLayer
                    .ToLocal(Player.GlobalPosition)); // big mess but just getting the players tile coord
        if ((_lastPlayerCoord - currPlayerCoord).LengthSquared() > 5 * 5)
        {
            if (_arrows.Count > 0)
                foreach (var ar in _arrows)
                    ar.QueueFree();


            CurrFlowField.GenerateFlowFieldFrom(currPlayerCoord);
            // for (int col = 0; col < CurrFlowField.Directions.GetLength(0); col++)
            // for (int row = 0; row < CurrFlowField.Directions.GetLength(1); row++)
            // {
            //     CreateLabel(col, row, $"{CurrFlowField.Costs[col, row]}");
            //     CreateArrow(col, row, CurrFlowField.Directions[col, row]);
            // }

            _lastPlayerCoord = currPlayerCoord;
        }
    }

    public void InitLevel()
    {
        SpawnNewWorld();
        SpawnPlayer();
        CurrFlowField = new FlowField(CurrWorld.LogicalData.Matrix,
            CurrWorld.LogicalData.NonWalkableState);
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