using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public class Shrine
    {
        public Vector2I RootCell { get; }
        public List<Vector2I> AllCells { get; }
        public int RowDir { get; }

        public int ColDir { get; }

        public int RowSize;

        public int ColSize;

        public Vector2I CenterTile { get; }

        public Shrine(List<Vector2I> allCells, Vector2I rootCell, int rowDir, int colDir, int rowSize, int colSize)
        {
            CenterTile = new Vector2I(
                rootCell.X + rowSize / 2 * rowDir,
                rootCell.Y + colSize / 2 * colDir
            );
            RootCell = rootCell;
            AllCells = allCells;
            RowDir = rowDir;
            ColDir = colDir;
            RowSize = rowSize;
            ColSize = colSize;
        }
    }
}