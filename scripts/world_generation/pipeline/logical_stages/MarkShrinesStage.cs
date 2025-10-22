using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages;

public partial class MarkShrinesStage : PipelineStage
{
    // Stage parameters
    [Export] private int _numShrines;
    [Export] private int _shrineSizeCols;
    [Export] private int _shrineSizeRows;
    [Export] private int _minDistance;
    
    // Local Members to cache Global Data (Logical World)
    private int[,] _matrix;
    private List<Island> _islands;
    private int _shrineState;
    
    // Stage result members
    private List<Shrine> _shrines;

    public override void ProcessWorld()
    {
        // cache needed references from Global Data
        _matrix = PipelineManager.LogicalWorldData.Matrix;
        _islands = PipelineManager.LogicalWorldData.Islands;
        _shrineState = PipelineManager.LogicalWorldData.ShrineState;
        
        // build config for stage params, init stage result members
        var shrineConfiguration = new ShrineConfig(_numShrines, _shrineSizeCols, _shrineSizeRows, _minDistance);
        _shrines = new List<Shrine>();
        
        // trigger stage logic 
        MarkShrinesWorldData(shrineConfiguration);

        // update Global Data
        PipelineManager.LogicalWorldData.Shrines = _shrines;
    }

    private Shrine CreateShrine(Vector2I rootCell, ShrineConfig shrineConfiguration)
    {
        List<Vector2I> allCells = new List<Vector2I>();
        int rowDirection = rootCell.X > shrineConfiguration.ShrineSizeCols ? -1 : 1;
        int colDirection = rootCell.Y > shrineConfiguration.ShrineSizeRows ? -1 : 1;
        for (int rowShift = 0; rowShift < shrineConfiguration.ShrineSizeRows; rowShift++)
        {
            for (int colShift = 0; colShift < shrineConfiguration.ShrineSizeCols; colShift++)
            {
                int currRow = rootCell.X + (rowShift * rowDirection);
                int currCol = rootCell.Y + (colShift * colDirection);
                Vector2I currCell = new Vector2I(currRow, currCol);
                allCells.Add(currCell);
            }
        }
        return new Shrine(allCells, rootCell, rowDirection, colDirection, shrineConfiguration.ShrineSizeRows, shrineConfiguration.ShrineSizeCols);
    }

    private void MarkShrinesWorldData(ShrineConfig shrineConfiguration)
    {
        int count = 0;
        while (_shrines.Count < shrineConfiguration.NumShrines && count < 100)
        {
            count++;
            Vector2I possibleShrinePlacement = _islands[GD.RandRange(0, _islands.Count - 1)].Centroid;
            bool validPlacement = true;
            foreach (Shrine currShrine in _shrines)
            {
                if ((possibleShrinePlacement - currShrine.RootCell).Length() < shrineConfiguration.MinDistance)
                {
                    validPlacement = false;
                    break;
                }
            }

            if (validPlacement)
            {
                Shrine newShrine = CreateShrine(possibleShrinePlacement, shrineConfiguration);
                _shrines.Add(newShrine);
            }
        }

        foreach (Shrine currShrine in _shrines)
        {
            MatrixUtils.InsertIsland(_matrix, currShrine.AllCells, _shrineState);
        }
    }
}