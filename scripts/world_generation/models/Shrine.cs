using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public class Shrine(List<Vector2I> allCells, Vector2I rootCell, int rowDir, int colDir)
    {
        public Vector2I RootCell { get; } = rootCell;
        public List<Vector2I> AllCells { get; } = allCells;
        public int RowDir { get; } = rowDir;
        public int ColDir { get; } = colDir;
    }
}