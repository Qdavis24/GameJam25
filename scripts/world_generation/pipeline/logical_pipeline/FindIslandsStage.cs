using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.pipeline.logical_pipeline
{
    public partial class FindIslandsStage : IPipelineStage
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

        private void DfsFloodFill(int row, int col, int state, List<Vector2I> allCells, bool[,] visited)
        {
            if (row < 0 || row >= _rowLength || col < 0 || col >= _colLength || visited[row, col]) return;

            visited[row, col] = true;
            if (_matrix[row, col] == state) allCells.Add(new Vector2I(col, row));
            else return;
            DfsFloodFill(row, col - 1, state, allCells, visited); // left
            DfsFloodFill(row, col + 1, state, allCells, visited); // right
            DfsFloodFill(row - 1, col, state, allCells, visited); // up
            DfsFloodFill(row + 1, col, state, allCells, visited); // down
            DfsFloodFill(row - 1, col - 1, state, allCells, visited); // up-left
            DfsFloodFill(row - 1, col + 1, state, allCells, visited); // up-right
            DfsFloodFill(row + 1, col - 1, state, allCells, visited); // down-left
            DfsFloodFill(row + 1, col + 1, state, allCells, visited); // down-right
        }

        public void FindIslands(int state)
        {
            Islands = new List<Island>();
            bool[,] visited = new bool[_rowLength, _colLength];
            for (int row = 0; row < _rowLength; row++)
            {
                for (int col = 0; col < _colLength; col++)
                {
                    List<Vector2I> allCells = new List<Vector2I>();
    
                    DfsFloodFill(row, col, state, allCells, visited);
                    if (allCells.Count > 0)
                    {
                        Island island = new Island(allCells);
                        Islands.Add(island);
                    }
                }
            }

        }
        
    }
}