using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameJam25.scripts.world_generation;


public partial class WorldSpawner : Node2D
{
    [ExportCategory("World Generation")] [Export]
    private int BorderSize;

    [Export] int MapSizeRows;
    [Export] int MapSizeCols;

    [Export] int[] PossibleStates; // All possible values that can exist in world cells (e.g., grass, water, mountain)

    [Export]
    float[] StateSpawnWeights; // Probability weights for each state during initial world generation (matched indexing)

    [Export] float TreeSpawnChance;
    [Export] int NumSmooths;

    [ExportCategory("Path Carving Settings")] [Export]
    Curve PathCurve; // shape of path

    [Export] int PathCurveSize; // Magnitude of path curves
    [Export] int PathRadius;

    [ExportCategory("Tiles")] [Export] TileMapLayer BaseTileMapLayer;
    [Export] TileMapLayer ObstacleTileMapLayer;
    [Export] TileConfig TileConfiguration;

    [ExportCategory("Shrines")] [Export] int NumShrines;
    [Export] int ShrineSizeRows;
    [Export] int ShrineSizeCols;
    [Export] int MinimumDistanceShrines; // in tile cells
    [Export] private PackedScene[] allShrinePckdScns; // Racoon index 0 , Own index 1, Rabbit index 2

    [ExportCategory("Player")] [Export] PackedScene PlayerScene;

    [ExportCategory("Enemy System")] [Export]
    private PackedScene _enemySpawner;

    private WorldGenerator world;

    private enum WorldDataStates
    {
        Walkable,
        NonWalkable,
        Shrine
    };

    private void PopulateBaseLayer()
    {
        for (int row = 0; row < world.Map.RowLength; row++)
        {
            for (int col = 0; col < world.Map.ColLength; col++)
            {
                int worldDataState = world.Map.Array[row, col];
                if (worldDataState == (int)WorldDataStates.Shrine) continue;
                Vector2I[] tileOptions = new Vector2I[] { };
                switch ((WorldDataStates)worldDataState)
                {
                    case WorldDataStates.Walkable:
                        tileOptions = TileConfiguration.BaseLayerWalkableTilesAtlasCoords;
                        break;
                    case WorldDataStates.NonWalkable:
                        tileOptions = TileConfiguration.BaseLayerNonWalkableTilesAtlasCoords;
                        break;
                }

                BaseTileMapLayer.SetCell(new Vector2I(row, col), 0, tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void PopulateObstacleLayer()
    {
        for (int row = 0; row < world.Map.RowLength; row++)
        {
            for (int col = 0; col < world.Map.ColLength; col++)
            {
                int worldDataState = world.Map.Array[row, col];
                if (GD.Randf() > TreeSpawnChance) continue;
                Vector2I[] tileOptions = new Vector2I[] { };
                switch ((WorldDataStates)worldDataState)
                {
                    case WorldDataStates.Walkable:
                        tileOptions = TileConfiguration.ObjectLayerWalkableTilesAtlasCoords;
                        break;
                    case WorldDataStates.NonWalkable:
                        tileOptions = TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords;
                        break;
                }

                if (GD.Randf() < TreeSpawnChance && worldDataState != (int)WorldDataStates.Shrine)
                    ObstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                        tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void CreateBorder()
    {
        // left pass
        for (int row = -BorderSize; row < MapSizeRows + BorderSize; row++)
        {
            for (int col = -BorderSize; col < 0; col++)
            {
                BaseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.BaseLayerNonWalkableTilesAtlasCoords[0]);
                ObstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }

        // top pass
        for (int row = -BorderSize; row < 0; row++)
        {
            for (int col = 0; col < MapSizeCols; col++)
            {
                BaseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.BaseLayerNonWalkableTilesAtlasCoords[0]);
                ObstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }

        //right pass
        for (int row = -BorderSize; row < MapSizeRows + BorderSize; row++)
        {
            for (int col = MapSizeCols; col < BorderSize + MapSizeCols; col++)
            {
                BaseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.BaseLayerNonWalkableTilesAtlasCoords[0]);
                ObstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }

        // bottom pass
        for (int row = MapSizeRows; row < MapSizeRows + BorderSize; row++)
        {
            for (int col = 0; col < MapSizeCols; col++)
            {
                BaseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.BaseLayerNonWalkableTilesAtlasCoords[0]);
                ObstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }
    }

    private void SpawnShrines()
    {
        for (int shrineIndex = 0; shrineIndex < allShrinePckdScns.Length; shrineIndex++)
        {
            Node2D enemySpawner = _enemySpawner.Instantiate<Node2D>();
            
            Node2D currShrine = allShrinePckdScns[shrineIndex].Instantiate<Node2D>();
            Vector2I centerTile = new Vector2I(
                world.Shrines[shrineIndex].RootCell.X + ShrineSizeRows / 2 * world.Shrines[shrineIndex].RowDir,
                world.Shrines[shrineIndex].RootCell.Y + ShrineSizeCols / 2 * world.Shrines[shrineIndex].ColDir
            );
            currShrine.Position = BaseTileMapLayer.MapToLocal(centerTile);
            enemySpawner.Position = BaseTileMapLayer.MapToLocal(centerTile);
            
            AddChild(enemySpawner);
            AddChild(currShrine);
        }
    }

    private void PopulateMap()
    {
        SpawnShrines();
        PopulateBaseLayer();
        PopulateObstacleLayer();
        CreateBorder();
    }

    private void WipeMap()
    {
        for (int row = 0; row < world.Map.RowLength; row++)
        {
            for (int col = 0; col < world.Map.ColLength; col++)
            {
                ObstacleTileMapLayer.EraseCell(new Vector2I(row, col));
                BaseTileMapLayer.EraseCell(new Vector2I(row, col));
            }
        }
    }

    private void SpawnPlayer()
    {
        // search tiles for a suitable spawn point
        for (int row = 0; row < world.Map.RowLength; row++)
        {
            for (int col = 0; col < world.Map.ColLength; col++)
            {
                // check if tile type is walkable for tile and surrounding tiles
                if (world.Map.Array[row, col] == (int)WorldDataStates.Walkable &&
                    MatrixUtils.UniformNeighbors(world.Map, row, col, 2, false))
                {
                    // spawn player
                    var player = (Node2D)PlayerScene.Instantiate();
                    player.GlobalPosition = ToGlobal(BaseTileMapLayer.MapToLocal(new Vector2I(row, col)));
                    AddChild(player);
                    return;
                }
            }
        }
    }

    public override void _Ready()
    {
        PathConfig pathConfig = new PathConfig(PathRadius, PathCurveSize, PathCurve);
        ShrineConfig shrineConfig =
            new ShrineConfig(NumShrines, ShrineSizeCols, ShrineSizeRows, MinimumDistanceShrines);
        world = new WorldGenerator(MapSizeCols, MapSizeRows, PossibleStates, StateSpawnWeights, NumSmooths,
            pathConfig, shrineConfig);
        PopulateMap();
        SpawnPlayer();
    }
}