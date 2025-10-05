using Godot;

namespace GameJam25.scripts.world_generation.configs
{
    
    public class ShrineConfig(int numShrines, int shrineSizeX, int shrineSizeY, int minDistance)
    {
        public int NumShrines { get; } = numShrines;
        public int ShrineSizeX { get; } = shrineSizeX;
        public int ShrineSizeY { get; } = shrineSizeY;
        public int MinDistance { get; } = minDistance;
    }
}