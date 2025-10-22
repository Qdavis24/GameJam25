using System;
using System.Collections.Generic;
using Godot;
using GameJam25.scripts.world_generation.configs;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages;

public partial class ConnectIslandsStage : PipelineStage
{
    // Stage parameters
    [Export] private Curve _pathCurve;
    [Export] private int _pathRadius;
    [Export] private int _pathCurveMagnitude;

    // Local Members to cache Global Data (Logical World)
    private List<IslandEdge> _islandConnections;
    private int[,] _matrix;

    private List<List<Vector2I>> _paths;

    public override void ProcessWorld()
    {
        // cache needed references from Global Data
        _islandConnections = PipelineManager.LogicalWorldData.IslandEdges;
        _matrix = PipelineManager.LogicalWorldData.Matrix;

        // build config for stage params (allows us to compress num of parameters)
        var pathConfig = new PathConfig(_pathRadius, _pathCurveMagnitude, _pathCurve);

        _paths = new List<List<Vector2I>>();

        // trigger stage logic 
        foreach (IslandEdge edge in _islandConnections)
        {
            var connectCells = FindClosestBorderCells(edge);
            DrawPathBetweenCells(connectCells[0], connectCells[1], pathConfig);
        }
            

        // update global data
        PipelineManager.LogicalWorldData.Paths = _paths;
    }

    private Vector2I[] FindClosestBorderCells(IslandEdge edge)
    {
        float minDistance = Mathf.Inf;
        Vector2I[] closestCells = new Vector2I[2];
        var island1BorderCells = edge.Island1.BorderCells;
        var island2BorderCells = edge.Island2.BorderCells;
        for (int cell1 = 0; cell1 < island1BorderCells.Count; cell1++)
        {
            for (int cell2 = cell1 + 1; cell2 < island2BorderCells.Count; cell2++)
            {
                float currDistance = (island1BorderCells[cell1] - island2BorderCells[cell2]).LengthSquared();
                if (currDistance < minDistance)
                {
                    minDistance = currDistance;
                    closestCells[0] = island1BorderCells[cell1];
                    closestCells[1] = island2BorderCells[cell2];
                }
            }
        }
        GD.Print(closestCells[0], closestCells[1]);
        return closestCells;
    }

    private void DrawPathBetweenCells(Vector2I startCell,
        Vector2I endCell, PathConfig pathConfig)
    {
        var currPath = new List<Vector2I>();
        Vector2 direction = endCell - startCell;

        // Calculate total steps needed
        int totalSteps = (int)Math.Max(Math.Abs(direction.X), Math.Abs(direction.Y));

        // Get perpendicular direction for curve offset
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X).Normalized();

        float currCurveSize = (float)totalSteps / 6;

        int state = _matrix[endCell.X, endCell.Y];

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
            int row = (int)Math.Round(curvedPos.X);
            int col = (int)Math.Round(curvedPos.Y);

            currPath.Add(new Vector2I(row, col));

            MatrixUtils.SetNeighbors(_matrix, row, col, pathConfig.PathRadius, state, true);
        }

        _paths.Add(currPath);
    }
}