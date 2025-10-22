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

        public override void ProcessWorld()
        {
            // cache needed references from Global Data
            _matrix = PipelineManager.LogicalWorldData.Matrix;
            _rowLength = PipelineManager.LogicalWorldData.RowLength;
            _colLength = PipelineManager.LogicalWorldData.ColLength;

            // trigger stage logic 
            FindIslands(_islandState);

            GD.Print("Island Count: " + Islands.Count);

            // update Global Data
            PipelineManager.LogicalWorldData.Islands = Islands;
        }

        private void DfsFloodFill(int row, int col, int state, Island island, bool[,] visited)
        {
            if (row < 0 || row >= _rowLength || col < 0 || col >= _colLength ||
                _matrix[row, col] != state)
                return;
            if (visited[row, col]) return;
            
            visited[row, col] = true;
            
            var currCell = new Vector2I(row, col);
            
            if (MatrixUtils.UniformNeighbors(_matrix, row, col, 1, false))
                island.AllCells.Add(currCell);
            else
                island.BorderCells.Add(currCell);
            
            DfsFloodFill(row, col - 1, state, island, visited); // left
            DfsFloodFill(row, col + 1, state, island, visited); // right
            DfsFloodFill(row - 1, col, state, island, visited); // up
            DfsFloodFill(row + 1, col, state, island, visited); // down
            DfsFloodFill(row - 1, col - 1, state, island, visited); // up-left
            DfsFloodFill(row - 1, col + 1, state, island, visited); // up-right
            DfsFloodFill(row + 1, col - 1, state, island, visited); // down-left
            DfsFloodFill(row + 1, col + 1, state, island, visited); // down-right
        }

        public void FindIslands(int state)
        {
            Islands = new List<Island>();
            bool[,] visited = new bool[_rowLength, _colLength];
            for (int row = 0; row < _rowLength; row++)
            {
                for (int col = 0; col < _colLength; col++)
                {
                    Island newIsland = new Island();
                    DfsFloodFill(row, col, state, newIsland, visited);
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