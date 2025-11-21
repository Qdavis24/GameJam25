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
		_worldGenPipelineManger.PipelineFinished += OnPipelineFinished;
		
		LogicalData = new();
		PhysicalData = new();
		_worldGenPipelineManger.RunPipeline();
	}

	private void OnPipelineFinished()
	{
		CleanupUnusedProceduralData();
		EmitSignalWorldGenerationComplete();
	}

	private void CleanupUnusedProceduralData()
	{
		LogicalData.Islands.Clear();
		LogicalData.IslandEdges.Clear();
		LogicalData.Paths.Clear();
	}
}
