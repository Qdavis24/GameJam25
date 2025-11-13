using Godot;

namespace GameJam25.scripts.world_generation.configs
{
    
    public class ShrineConfig(int numShrines, int shrineSizeCols, int shrineSizeRows, int minDistance)
    {
        public int NumShrines { get; } = numShrines;
        public int ShrineSizeCols { get; } = shrineSizeCols;
        public int ShrineSizeRows { get; } = shrineSizeRows;
        public int MinDistance { get; } = minDistance;
    }
}