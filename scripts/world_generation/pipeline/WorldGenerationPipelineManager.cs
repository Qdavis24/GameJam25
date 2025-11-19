using System.Collections.Generic;
using GameJam25.scripts.world_generation.models;
using GameJam25.scripts.world_generation.pipeline;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline
/*
 * Pipeline Manager is a coordinator that ensures each stage of the pipeline
 * fulfills its purpose.
 *
 * It gathers its stages via GetChildren
 *
 * (Important Note)
 * the stages must be inserted as children IN THE ORDER they should execute
 *
 * (Another Important Note)
 * Logical World Data and Physical World Data act as data buckets for stages to share information
 * they are propagated to the root World Node for access from external nodes in scene tree after generation
 * 
 * 
 * the full pipeline flow is as follows
 *
 * 1) initialize a matrix and populate each cell with states based on that states probability
 * 2) find all connected components for a given state (islands) - more simply put disconnected regions of a state
 * 3) create a collection of all possible connections between the islands and sort by distance
 * 4) create a collection of the minimum required connections (edges) between the islands to connect them all
 * 5) connect the islands by writing a path between them of their state into the matrix
 * 6) mark shrines (handcrafted scenes) into the matrix with a minimum distance apart from eachother
 * 7) render the tiles on the tile map layers
 * 8) spawn the shrine scenes
 * 9) spawn upgrade chests
 * 10) find player spawn
 */
{
	public partial class WorldGenerationPipelineManager : Node
	{
		[Signal]
		public delegate void PipelineFinishedEventHandler();
		[Export] public bool Debug;
		private List<PipelineStage> _pipelineStages;

		public void RunPipeline()
		{
			_pipelineStages = new List<PipelineStage>();
			
			foreach (Node child in GetChildren())
			{
				_pipelineStages.Add(child as PipelineStage);
			}
			
			foreach (PipelineStage stage in _pipelineStages) stage.ProcessStage();
			EmitSignalPipelineFinished();
		}
	}
}
