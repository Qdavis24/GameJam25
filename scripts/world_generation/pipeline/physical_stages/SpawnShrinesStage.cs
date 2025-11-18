using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline.physical_stages;

public partial class SpawnShrinesStage : PipelineStage
{
    // Stage parameters
    [Export] private PackedScene[] _allShrinePackedScenes;

    // Local Members to cache Global Data (Logical World)
    private List<Shrine> _shrines;
    // Local Members to cache Global Data (Physical World)
    private TileMapLayer _baseTileMapLayer;


    public override void ProcessStage()
    {
        // cache needed references from Global Data
        _shrines = World.LogicalData.Shrines;
        _baseTileMapLayer = World.PhysicalData.BaseTileMapLayer;
        
        // trigger stage logic
        SpawnShrines();
    }

    private void SpawnShrines()
    {
        for (int i = 0; i < _shrines.Count; i++)
        {
            Node2D currShrine = _allShrinePackedScenes[i].Instantiate<Node2D>();
            
            currShrine.Position = _baseTileMapLayer.MapToLocal(_shrines[i].CenterTile);
            Callable.From(() => _baseTileMapLayer.AddSibling(currShrine)).CallDeferred();
            
        }
    }
}