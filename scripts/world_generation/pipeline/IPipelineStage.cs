using System.Diagnostics;
using Godot;

namespace GameJam25.scripts.world_generation.pipeline;

/*
 * This is an abstract class functioning as a contract forcing all subtypes to
 * implement the necessary ProcessWorld Method
 *
 * Each stage is responsible for carrying out a single step in the multistep
 * process of generating a procedural world
 *
 * Each stage will have its own customizable parameters that will affect
 * how it performs its task via Export Variables
 *
 * for example
 * ConnectIslandsStage requires specifying a curve to know how to draw its path
 *
 * The idea is to confine all single stage related parameters to the stage itself
 *
 * each stage will follow the same flow
 *  - read dependencies from logical and physical world required for the step
 *  - carry out some logic that is required of that step
 *  - write back any new information to logical and physical world
 *
 * (Important Note) because we are passing the matrix to each stage via ref no write back
 * is required for mutations to the matrix. With that note some stages don't need a writeback
 *
 * (see logical and physical world for more info on their purpose)
 *
 * for more information on the pipeline see WorldGenerationPipelineManager
 *
 */
public abstract partial class IPipelineStage : Node
{
    protected WorldGenerationPipelineManager PipelineManager;
    public override void _Ready()
    {
        PipelineManager = (WorldGenerationPipelineManager) GetParent();
    }
    public abstract void ProcessWorld();
}