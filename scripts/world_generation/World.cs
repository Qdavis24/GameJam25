using Godot;
using System;
using GameJam25.scripts.world_generation.pipeline;

namespace GameJam25.scripts.world_generation;
public partial class World : Node2D
{
	[Signal] public delegate void WorldGenerationCompleteEventHandler();
	
	[Export] private WorldGenerationPipelineManager _worldGenPipelineManger;
	[Export] public bool Debug;
	
	public LogicalWorldData LogicalData;
	public PhysicalWorldData PhysicalData;

	public override void _Ready()
	{
		_worldGenPipelineManger.PipelineFinished += EmitSignalWorldGenerationComplete;
		
		LogicalData = new();
		PhysicalData = new();
		_worldGenPipelineManger.RunPipeline();
		
	}
}
