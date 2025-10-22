using System.Collections.Generic;
using Godot;
namespace GameJam25.scripts.world_generation.pipeline.physical_stages;

public partial class RenderTilesStage : PipelineStage
{
    // Stage parameters
    [Export] private TileMapLayer _baseTileMapLayer;
    [Export] private TileMapLayer _obstacleTileMapLayer;
    [Export] private TileConfig _tileConfig;
    [Export] private float _obstacleSpawnChance = .9f;
    [Export] private int _borderSize = 20;
    [Export] private int _neighborsPerSpawn;
 
    // Local Members to cache Global Data (Logical World)
    private int[,] _matrix;
    private int _rowLength;
    private int _colLength;
    private int _walkableState;
    private int _nonWalkableState;
    private int _shrineState;
    
    public override void ProcessWorld()
    {
        // cache needed references from Global Data
        _matrix = PipelineManager.LogicalWorldData.Matrix;
        _rowLength = PipelineManager.LogicalWorldData.RowLength;
        _colLength = PipelineManager.LogicalWorldData.ColLength;
        _walkableState = PipelineManager.LogicalWorldData.WalkableState;
        _nonWalkableState = PipelineManager.LogicalWorldData.NonWalkableState;
        _shrineState = PipelineManager.LogicalWorldData.ShrineState;
        
        // trigger stage logic
        PopulateBaseLayer();
        PopulateObstacleLayer();
        CreateBorder();

        if (PipelineManager.Debug)
        {
            PopulateIslands();
            PopulateIslandBorders();
            PopulatePaths();
        }
        
        // update Global Data
        PipelineManager.PhysicalWorldData.BaseTileMapLayer =  _baseTileMapLayer;
        PipelineManager.PhysicalWorldData.ObstacleTileMapLayer = _obstacleTileMapLayer;
        PipelineManager.PhysicalWorldData.TileConfiguration =  _tileConfig;
        

    }

    private void PopulateIslands()
    {
        foreach (Island island in PipelineManager.LogicalWorldData.Islands)
        {
            foreach (Vector2I islandCell in island.AllCells)
            {
                _baseTileMapLayer.SetCell(islandCell, 0, _tileConfig.DebugIslandAtlasCoord);
            }
        }
    }
    
    private void PopulateIslandBorders()
    {
        foreach (Island island in PipelineManager.LogicalWorldData.Islands)
        {
            foreach (Vector2I islandCell in island.BorderCells)
            {
                _baseTileMapLayer.SetCell(islandCell, 0, _tileConfig.DebugIslandBorderAtlasCoord);
            }
        }
    }

    private void PopulatePaths()
    {
        foreach (List<Vector2I> path in PipelineManager.LogicalWorldData.Paths)
        {
            foreach (Vector2I pathCell in path)
            {
                _baseTileMapLayer.SetCell(pathCell, 0, _tileConfig.DebugPathAtlasCoord);
            }
        }
    }
    
    private void PopulateBaseLayer()
    {
        for (int row = 0; row < _rowLength; row++)
        {
            for (int col = 0; col < _colLength; col++)
            {
                int worldDataState = _matrix[row, col];
                if (worldDataState == _shrineState) continue;
                Vector2I[] tileOptions = new Vector2I[] { };
                if (worldDataState == _walkableState) tileOptions = _tileConfig.BaseLayerWalkableTilesAtlasCoords;
                else tileOptions = _tileConfig.BaseLayerNonWalkableTilesAtlasCoords;
                _baseTileMapLayer.SetCell(new Vector2I(row, col), 0, tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void PopulateObstacleLayer()
    {
        for (int row = 0; row < _rowLength; row++)
        {
            for (int col = 0; col < _colLength; col++)
            {
                int worldDataState = _matrix[row, col];
                if (GD.Randf() > _obstacleSpawnChance) continue;
                Vector2I[] tileOptions = new Vector2I[] { };
                if (worldDataState == _walkableState)tileOptions = _tileConfig.ObjectLayerWalkableTilesAtlasCoords;
                else tileOptions = _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords;
                
                if (GD.Randf() < _obstacleSpawnChance && worldDataState != _shrineState && MatrixUtils.UniformNeighbors(_matrix, row, col, _neighborsPerSpawn, true))
                    _obstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                        tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void CreateBorder()
    {
        // left pass
        for (int row = -_borderSize; row < _rowLength + _borderSize; row++)
        {
            for (int col = -_borderSize; col < 0; col++)
            {
                _baseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
                _obstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }

        // top pass
        for (int row = -_borderSize; row < 0; row++)
        {
            for (int col = 0; col < _colLength; col++)
            {
                _baseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
                _obstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }

        //right pass
        for (int row = -_borderSize; row < _rowLength + _borderSize; row++)
        {
            for (int col = _colLength; col < _borderSize + _colLength; col++)
            {
                _baseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
                _obstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }

        // bottom pass
        for (int row = _rowLength; row < _rowLength + _borderSize; row++)
        {
            for (int col = 0; col < _colLength; col++)
            {
                _baseTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
                _obstacleTileMapLayer.SetCell(new Vector2I(row, col), 0,
                    _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
                        GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
            }
        }
    }
}