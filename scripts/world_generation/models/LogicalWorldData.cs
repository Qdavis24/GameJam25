using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public struct LogicalWorldData
    {
        public LogicalWorldData()
        {
            
        }
        public int[,] Matrix; // col major
        public List<Island> Islands;
        public List<IslandEdge> IslandEdges;
        public List<Shrine> Shrines;
        public List<List<Vector2I>> Paths;
        public int RowLength;
        public int ColLength;
        public int[] States;
        public float[] StateProbabilities;
        public int WalkableState = 0;
        public int NonWalkableState = 1;
        public int ShrineState = -1;
        public Vector2 PlayerSpawn;

    }
}