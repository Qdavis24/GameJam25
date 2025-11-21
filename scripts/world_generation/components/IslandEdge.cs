using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public struct IslandEdge(Island island1, Island island2, float distance)
    {
        public Island Island1 { get; } = island1;
        public Island Island2 { get; } = island2;
        public float Distance { get; } = distance;
    }
}