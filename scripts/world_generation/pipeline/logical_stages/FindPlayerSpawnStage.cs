using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages;

public partial class FindPlayerSpawnStage : PipelineStage
{
    [Export] private float _minDistanceFromShrines = 30;

    private List<Island> _islands;
    private List<Shrine> _shrines;
    private TileMapLayer _baseTileMapLayer;

    private Vector2 _playerSpawn;

    public override void ProcessWorld()
    {
        _islands = PipelineManager.LogicalWorldData.Islands;
        _shrines = PipelineManager.LogicalWorldData.Shrines;
        _baseTileMapLayer = PipelineManager.PhysicalWorldData.BaseTileMapLayer;
        FindValidSpawn();
        PipelineManager.LogicalWorldData.PlayerSpawn = _playerSpawn;
    }

    private void FindValidSpawn()
    {
        foreach (Island island in _islands)
        {
            bool valid = true;
            foreach (Shrine shrine in _shrines)
            {
                var sampleCell = (island.AllCells[island.AllCells.Count / 2] - shrine.CenterTile);
                if (sampleCell.LengthSquared() <
                    _minDistanceFromShrines * _minDistanceFromShrines)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                _playerSpawn = _baseTileMapLayer.ToGlobal(_baseTileMapLayer.MapToLocal(island.Centroid));
                return;
            }
        }
    }
}