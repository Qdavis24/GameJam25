using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline.physical_stages
{
	public partial class SpawnChestsStage : PipelineStage
	{
		// Stage parameter: chest scene(s) to spawn
		[Export] private PackedScene _chestPackedScene;
		// Or if you want variety:
		// [Export] private PackedScene[] _allChestPackedScenes;

		// Cached logical/physical data
		private List<Island> _islands;
		private TileMapLayer _baseTileMapLayer;

		public override void ProcessStage()
		{
			// cache needed references from Global Data
			_islands = World.LogicalData.Islands;
			_baseTileMapLayer = World.PhysicalData.BaseTileMapLayer;

			if (_islands == null || _islands.Count == 0 || _chestPackedScene == null)
				return;

			SpawnChests();
		}

		private void SpawnChests()
		{
			for (int i = 0; i < _islands.Count; i++)
			{
				Island island = _islands[i];

				// Centroid is a Vector2I in tile/matrix space
				Vector2I centerTile = island.Centroid;

				// Convert tile coordinate -> world position using the TileMap
				Vector2 worldPos = _baseTileMapLayer.MapToLocal(centerTile);

				// Pick a chest scene
				Node2D chest = _chestPackedScene.Instantiate<Node2D>();
				// Or, if you use an array and want variety:
				// var chestScene = _allChestPackedScenes[i % _allChestPackedScenes.Length];
				// Node2D chest = chestScene.Instantiate<Node2D>();

				chest.Position = worldPos;

				// Add next to the tilemap like your shrines
				Callable.From(() => _baseTileMapLayer.AddSibling(chest)).CallDeferred();
			}
		}
	}
}
