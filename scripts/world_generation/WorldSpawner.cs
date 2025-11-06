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


    [ExportCategory("Shrines")] [Export] int NumShrines;
    [Export] int ShrineSizeRows;
    [Export] int ShrineSizeCols;
    [Export] int MinimumDistanceShrines; // in tile cells
    [Export] private PackedScene[] allShrinePckdScns; // Racoon index 0 , Own index 1, Rabbit index 2

    [ExportCategory("Player")] [Export] PackedScene PlayerScene;

    [ExportCategory("Enemy System")] [Export]
    private PackedScene _enemySpawner;

    // private WorldGenerator world;

    private enum WorldDataStates
    {
        Walkable,
        NonWalkable,
        Shrine
    };
    
    // private void WipeMap()
    // {
    //     for (int row = 0; row < world.Map.RowLength; row++)
    //     {
    //         for (int col = 0; col < world.Map.ColLength; col++)
    //         {
    //             ObstacleTileMapLayer.EraseCell(new Vector2I(row, col));
    //             BaseTileMapLayer.EraseCell(new Vector2I(row, col));
    //         }
    //     }
    // }

    // private void SpawnPlayer()
    // {
    //     // search tiles for a suitable spawn point
    //     for (int row = 0; row < world.Map.RowLength; row++)
    //     {
    //         for (int col = 0; col < world.Map.ColLength; col++)
    //         {
    //             // check if tile type is walkable for tile and surrounding tiles
    //             if (world.Map.Array[row, col] == (int)WorldDataStates.Walkable &&
    //                 MatrixUtils.UniformNeighbors(world.Map, row, col, 2, false))
    //             {
    //                 // spawn player
    //                 var player = (Node2D)PlayerScene.Instantiate();
    //                 player.GlobalPosition = ToGlobal(BaseTileMapLayer.MapToLocal(new Vector2I(row, col)));
    //                 AddChild(player);
    //                 return;
    //             }
    //         }
    //     }
    // }


}