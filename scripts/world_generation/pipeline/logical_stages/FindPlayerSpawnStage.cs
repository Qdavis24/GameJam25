﻿using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages;

public partial class FindPlayerSpawnStage : PipelineStage
{
    [Export] private float _minDistanceFromShrines = 30;

    private int[,] _matrix;
    private int _rowLength;
    private int _colLength;
    private TileMapLayer _baseTileMapLayer;

    private Vector2 _playerSpawn;

    public override void ProcessWorld()
    {
        _matrix = PipelineManager.LogicalWorldData.Matrix;
        _rowLength = PipelineManager.LogicalWorldData.RowLength;
        _colLength = PipelineManager.LogicalWorldData.ColLength;
        _baseTileMapLayer = PipelineManager.PhysicalWorldData.BaseTileMapLayer;
        FindValidSpawn();
        PipelineManager.LogicalWorldData.PlayerSpawn = _playerSpawn;
    }

    private void FindValidSpawn()
    {
        int count = 0;
        while (count < 100)
        {
            count++;
            Vector2I potentialSpawn = new Vector2I(GD.RandRange(0, _rowLength-1), GD.RandRange(0, _colLength-1));
            if (_matrix[potentialSpawn.X, potentialSpawn.Y] == PipelineManager.LogicalWorldData.WalkableState && MatrixUtils.UniformNeighbors(_matrix, potentialSpawn.X, potentialSpawn.Y, 4, false))
            {
                _playerSpawn = _baseTileMapLayer.ToGlobal(_baseTileMapLayer.MapToLocal(potentialSpawn));
                return;
            }
        }
    }
}