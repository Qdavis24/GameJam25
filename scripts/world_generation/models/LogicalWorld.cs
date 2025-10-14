using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public partial class LogicalWorld
    {
        public int[,] Matrix;
        public List<Island> Islands;
        public List<IslandEdge> IslandEdges;
        public List<Shrine> Shrines;
        public int RowLength { get; set; }
        public int ColLength { get; set; }
        public int[] States;
        public float[] StateProbabilities;
        public int WalkableState = 0;
        public int NonWalkableState = 1;
        public int ShrineState = -1;

    }
}