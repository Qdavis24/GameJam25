using System.Collections.Generic;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline.physical_pipeline;

public partial class SpawnShrinesStage : IPipelineStage
{
    // Stage parameters
    [Export] private PackedScene[] _allShrinePackedScenes;

    // Local Members to cache Global Data (Logical World)
    private List<Shrine> _shrines;
    // Local Members to cache Global Data (Physical World)
    private TileMapLayer _baseTileMapLayer;


    public override void ProcessWorld()
    {
        // cache needed references from Global Data
        _shrines = PipelineManager.LogicalWorldData.Shrines;
        _baseTileMapLayer = PipelineManager.PhysicalWorldData.BaseTileMapLayer;
        
        // trigger stage logic
        SpawnShrines();
    }

    private void SpawnShrines()
    {
        for (int i = 0; i < _shrines.Count; i++)
        {
            Node2D currShrine = _allShrinePackedScenes[i].Instantiate<Node2D>();
            
            GD.Print(_shrines[i].CenterTile);
            currShrine.Position = _baseTileMapLayer.MapToLocal(_shrines[i].CenterTile);
            Callable.From(() => _baseTileMapLayer.AddSibling(currShrine)).CallDeferred();
            
        }
    }
}