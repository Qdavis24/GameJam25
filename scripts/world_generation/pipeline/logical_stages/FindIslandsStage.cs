using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages
{
    public partial class FindIslandsStage : PipelineStage
    {
        // Stage parameters
        [Export] private int _islandState;

        // Local Members to cache Global Data (Logical World)
        private int[,] _matrix;
        private int _rowLength;
        private int _colLength;

        // Stage result members
        public List<Island> Islands { get; private set; }

        public override void ProcessStage()
        {
            // cache needed references from Global Data
            _matrix = World.LogicalData.Matrix;
            _rowLength = World.LogicalData.RowLength;
            _colLength = World.LogicalData.ColLength;

            // trigger stage logic 
            FindIslands(_islandState);

            GD.Print("Island Count: " + Islands.Count);

            // update Global Data
            World.LogicalData.Islands = Islands;
        }

        private void DfsFloodFill(int col, int row, int state, Island island, bool[,] visited)
        {
            if (col < 0 || col >= _colLength || row < 0 || row >= _rowLength ||
                _matrix[col, row] != state)
                return;
            if (visited[col, row]) return;

            visited[col, row] = true;

            var currCell = new Vector2I(col, row);

            if (MatrixUtils.UniformNeighbors(_matrix, col, row, 1, false))
                island.AllCells.Add(currCell);
            else
                island.BorderCells.Add(currCell);

            DfsFloodFill(col - 1, row, state, island, visited); // left
            DfsFloodFill(col + 1, row, state, island, visited); // right
            DfsFloodFill(col, row - 1, state, island, visited); // up
            DfsFloodFill(col, row + 1, state, island, visited); // down
            DfsFloodFill(col - 1, row - 1, state, island, visited); // up-left
            DfsFloodFill(col + 1, row - 1, state, island, visited); // up-right
            DfsFloodFill(col - 1, row + 1, state, island, visited); // down-left
            DfsFloodFill(col + 1, row + 1, state, island, visited); // down-right
        }

        public void FindIslands(int state)
        {
            Islands = new List<Island>();
            bool[,] visited = new bool[_colLength, _rowLength];
            for (int col = 0; col < _colLength; col++)
            {
                for (int row = 0; row < _rowLength; row++)
                {
                    Island newIsland = new Island();
                    DfsFloodFill(col, row, state, newIsland, visited);
                    if (newIsland.AllCells.Count > 0)
                    {
                        newIsland.CalculateCentroid();
                        Islands.Add(newIsland);
                    }
                }
            }
        }
    }
}