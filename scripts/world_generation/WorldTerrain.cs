using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameJam25.scripts.world_generation;

public partial class WorldTerrain : Node2D
{
    [ExportCategory("World Generation")] 
    [Export] int MapSizeRows;
    [Export] int MapSizeCols;
    
    [Export] int[] PossibleStates; // All possible values that can exist in world cells (e.g., grass, water, mountain)
    [Export] float[] StateSpawnWeights; // Probability weights for each state during initial world generation (matched indexing)

    [Export] float TreeSpawnChance;
    [Export] int NumSmooths;

    [ExportCategory("Path Carving Settings")] 
    [Export] Curve PathCurve; // shape of path
    [Export] int PathCurveSize; // Magnitude of path curves
    [Export] int PathRadius;
    
    [ExportCategory("Tiles")] 
    [Export] TileMapLayer BaseTileMapLayer;
    [Export] TileMapLayer ObjectTileMapLayer;
    [Export] TileConfig TileConfiguration;

    [ExportCategory("Shrines")] 
    [Export] int NumShrines;
    [Export] int ShrineSizeRows;
    [Export] int ShrineSizeCols;
    [Export] int MinimumDistanceShrines; // in tile cells
    [Export] private PackedScene[] allShrinePckdScns; // Racoon index 0 , Own index 1, Rabbit index 2


    private WorldGenerator world;

    private enum WorldDataStates
    {
        Walkable,
        NonWalkable,
        Shrine
    };

    private void populateBaseLayer()
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

    private void populateObjectLayer()
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

                if (GD.Randf() < TreeSpawnChance && worldDataState != (int) WorldDataStates.Shrine)
                    ObjectTileMapLayer.SetCell(new Vector2I(row, col), 0, tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void spawnShrines()
    {
        for (int shrine = 0; shrine < allShrinePckdScns.Length; shrine++)
        {
            Node2D currShrine = allShrinePckdScns[shrine].Instantiate<Node2D>();
            Vector2I centerTile = new Vector2I(
                world.Shrines[shrine].AllCells[0].X + (ShrineSizeRows / 2),
                world.Shrines[shrine].AllCells[0].Y + (ShrineSizeCols / 2)
            );
            currShrine.Position = BaseTileMapLayer.MapToLocal(centerTile);
            AddChild(currShrine);
        }
    }

    private void populateMap()
    {
        populateBaseLayer();
        populateObjectLayer();
        spawnShrines();
    }

    private void wipeMap()
    {
        for (int row = 0; row < world.Map.RowLength; row++)
        {
            for (int col = 0; col < world.Map.ColLength; col++)
            {
                ObjectTileMapLayer.EraseCell(new Vector2I(row, col));
                BaseTileMapLayer.EraseCell(new Vector2I(row, col));
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
        populateMap();
    }
}