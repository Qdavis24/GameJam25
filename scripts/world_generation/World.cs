using Godot;
using System;
using GameJam25.scripts.world_generation.pipeline;

namespace GameJam25.scripts.world_generation;
public partial class World : Node2D
{
    [Export] private WorldGenerationPipelineManager _worldGenPipelineManger;
    [Export] public bool Debug;
    
    public LogicalWorldData LogicalData;
    public PhysicalWorldData PhysicalData;

    public override void _Ready()
    {
        LogicalData = new();
        PhysicalData = new();
        _worldGenPipelineManger.RunPipeline();
    }
}