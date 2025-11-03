using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.world_generation.pipeline;

namespace GameJam25.scripts.world_generation;

public partial class World : Node2D
{
    [Export] private WorldGenerationPipelineManager _worldGenPipelineManger;
    [Export] public bool Debug;

    public int[,] Matrix;
    public List<Island> Islands;
    public List<IslandEdge> IslandEdges;
    public List<Shrine> Shrines;
    public List<List<Vector2I>> Paths;
    public int RowLength;
    public int ColLength;
    public int[] States;
    public float[] StateProbabilities;
    public int WalkableState = 0;
    public int NonWalkableState = 1;
    public int ShrineState = -1;
    public Vector2 PlayerSpawn;
    public TileMapLayer BaseTileMapLayer;
    public TileMapLayer ObstacleTileMapLayer;
    public TileConfig TileConfiguration;

    public override void _Ready()
    {
        _worldGenPipelineManger.RunPipeline();
    }
}