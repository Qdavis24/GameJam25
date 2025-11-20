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
	
	public override void ProcessStage()
	{
		// cache needed references from Global Data
		_matrix = World.LogicalData.Matrix;
		_rowLength = World.LogicalData.RowLength;
		_colLength = World.LogicalData.ColLength;
		_walkableState = World.LogicalData.WalkableState;
		_nonWalkableState = World.LogicalData.NonWalkableState;
		_shrineState = World.LogicalData.ShrineState;
		
		// trigger stage logic
		PopulateBaseLayer();
		PopulateObstacleLayer();
		CreateBorder();

		if (World.Debug)
		{
			PopulateIslands();
			PopulateIslandBorders();
			PopulatePaths();
		}
		
		// update Global Data
		World.PhysicalData.BaseTileMapLayer =  _baseTileMapLayer;
		World.PhysicalData.ObstacleTileMapLayer = _obstacleTileMapLayer;
		World.PhysicalData.TileConfiguration =  _tileConfig;
		

	}

	private void PopulateIslands()
	{
		foreach (Island island in World.LogicalData.Islands)
		{
			foreach (Vector2I islandCell in island.AllCells)
			{
				_baseTileMapLayer.SetCell(islandCell, 0, _tileConfig.DebugIslandAtlasCoord);
			}
		}
	}
	
	private void PopulateIslandBorders()
	{
		foreach (Island island in World.LogicalData.Islands)
		{
			foreach (Vector2I islandCell in island.BorderCells)
			{
				_baseTileMapLayer.SetCell(islandCell, 0, _tileConfig.DebugIslandBorderAtlasCoord);
			}
		}
	}

	private void PopulatePaths()
	{
		foreach (List<Vector2I> path in World.LogicalData.Paths)
		{
			foreach (Vector2I pathCell in path)
			{
				_baseTileMapLayer.SetCell(pathCell, 0, _tileConfig.DebugPathAtlasCoord);
			}
		}
	}
	
	private void PopulateBaseLayer()
	{
		for (int col = 0; col < _colLength; col++)
		{
			for (int row = 0; row < _rowLength; row++)
			{
				int worldDataState = _matrix[col, row];
				if (worldDataState == _shrineState) continue;
				Vector2I[] tileOptions = new Vector2I[] { };
				if (worldDataState == _walkableState) tileOptions = _tileConfig.BaseLayerWalkableTilesAtlasCoords;
				else tileOptions = _tileConfig.BaseLayerNonWalkableTilesAtlasCoords;
				_baseTileMapLayer.SetCell(new Vector2I(col, row), 0, tileOptions[GD.Randi() % tileOptions.Length]);
			}
		}
	}

	private void PopulateObstacleLayer()
	{
		for (int col = 0; col < _colLength; col++)
		{
			for (int row = 0; row < _rowLength; row++)
			{
				int worldDataState = _matrix[col, row];
				if (GD.Randf() > _obstacleSpawnChance) continue;
				Vector2I[] tileOptions = new Vector2I[] { };
				if (worldDataState == _walkableState) continue;
				tileOptions = _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords;

				if (GD.Randf() < _obstacleSpawnChance && worldDataState != _shrineState && MatrixUtils.UniformNeighbors(_matrix, col, row, _neighborsPerSpawn, true))
					_obstacleTileMapLayer.SetCell(new Vector2I(col, row), 1,
						tileOptions[GD.Randi() % tileOptions.Length]);
			}
		}
	}

	private void CreateBorder()
	{
		// left pass
		for (int col = -_borderSize; col < 0; col++)
		{
			for (int row = -_borderSize; row < _rowLength + _borderSize; row++)
			{
				_baseTileMapLayer.SetCell(new Vector2I(col, row), 0,
					_tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
				_obstacleTileMapLayer.SetCell(new Vector2I(col, row), 1,
					_tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
						GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
			}
		}

		// top pass
		for (int col = 0; col < _colLength; col++)
		{
			for (int row = -_borderSize; row < 0; row++)
			{
				_baseTileMapLayer.SetCell(new Vector2I(col, row), 0,
					_tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
				_obstacleTileMapLayer.SetCell(new Vector2I(col, row), 1,
					_tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
						GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
			}
		}

		//right pass
		for (int col = _colLength; col < _borderSize + _colLength; col++)
		{
			for (int row = -_borderSize; row < _rowLength + _borderSize; row++)
			{
				_baseTileMapLayer.SetCell(new Vector2I(col, row), 0,
					_tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
				_obstacleTileMapLayer.SetCell(new Vector2I(col, row), 1,
					_tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
						GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
			}
		}

		// bottom pass
		for (int col = 0; col < _colLength; col++)
		{
			for (int row = _rowLength; row < _rowLength + _borderSize; row++)
			{
				_baseTileMapLayer.SetCell(new Vector2I(col, row), 0,
					_tileConfig.BaseLayerNonWalkableTilesAtlasCoords[0]);
				_obstacleTileMapLayer.SetCell(new Vector2I(col, row), 1,
					_tileConfig.ObjectLayerNonWalkableTilesAtlasCoords[
						GD.Randi() % _tileConfig.ObjectLayerNonWalkableTilesAtlasCoords.Length]);
			}
		}
	}
}
