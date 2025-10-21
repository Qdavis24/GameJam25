using System;
using System.Collections.Generic;
using Godot;
using GameJam25.scripts.world_generation.configs;

namespace GameJam25.scripts.world_generation.pipeline.logical_pipeline;

public partial class ConnectIslandsStage : IPipelineStage
{
    // Stage parameters
    [Export] private Curve _pathCurve;
    [Export] private int _pathRadius;
    [Export] private int _pathCurveMagnitude;
    
    // Local Members to cache Global Data (Logical World)
    private List<IslandEdge> _islandConnections;
    private int[,] _matrix;
    
    public override void ProcessWorld()
    {
        // cache needed references from Global Data
        _islandConnections = PipelineManager.LogicalWorldData.IslandEdges;
        _matrix = PipelineManager.LogicalWorldData.Matrix;
        
        // build config for stage params (allows us to compress num of parameters)
        var pathConfig = new PathConfig(_pathRadius, _pathCurveMagnitude, _pathCurve);
        
        // trigger stage logic 
        foreach(IslandEdge edge in _islandConnections)
            DrawPathBetweenCells(edge.Island1.Centroid, edge.Island2.Centroid, pathConfig);
    }

    public void DrawPathBetweenCells(Vector2I startCell,
        Vector2I endCell, PathConfig pathConfig)
    {
        Vector2 direction = endCell - startCell;
        
        // Calculate total steps needed
        int totalSteps = (int)Math.Max(Math.Abs(direction.X), Math.Abs(direction.Y));
        if (totalSteps == 0) return; // Same position

        // Get perpendicular direction for curve offset
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X).Normalized();

        float currCurveSize = (float) totalSteps / 6;

        int state = _matrix[endCell.Y, endCell.X];

        for (int step = 0; step <= totalSteps; step++)
        {
            float progress = (float)step / totalSteps; // 0 to 1
            

            // Get straight line position
            Vector2 straightPos = startCell + direction * progress;

            // Sample curve for bend amount
            float bendAmount = pathConfig.pathCurve.Sample(progress);

            // Apply perpendicular offset
            Vector2 curvedPos = straightPos + perpendicular * bendAmount * currCurveSize;

            // Convert to grid coordinates
            int row = (int)Math.Round(curvedPos.Y);
            int col = (int)Math.Round(curvedPos.X);

            // Place the path tile
            MatrixUtils.SetNeighbors(_matrix, row, col, pathConfig.PathRadius, state, true);
        }
    }
}