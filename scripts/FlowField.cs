using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class FlowField
{
    public int[,] Costs;
    public Vector2[,] Directions;

    private bool[,] _visited;

    private int[,] _terrainMatrix;
    private int _nonWalkableCellState;
    private int _terrainRowLength;
    private int _terrainColumnLength;


    public FlowField(int[,] terrainMatrix, int nonWalkableCellState)
    {
        _terrainMatrix = terrainMatrix;
        _nonWalkableCellState = nonWalkableCellState;

        _terrainColumnLength = _terrainMatrix.GetLength(0);
        _terrainRowLength = _terrainMatrix.GetLength(1);
    }

    public void GenerateFlowFieldFrom(Vector2I coord)
    {
        _visited = new bool[_terrainColumnLength, _terrainRowLength];
        Costs = new int[_terrainColumnLength, _terrainRowLength];
        for(int col =0;col<_terrainColumnLength;col++)
        for (int row = 0; row < _terrainRowLength; row++)
            Costs[col, row] = int.MaxValue;
        Directions = new Vector2[_terrainColumnLength, _terrainRowLength];

        GenCostMatrix(coord);
        GenDirectionMatrix();
    }

    private void GenCostMatrix(Vector2I startCoord)
    {
        var frontier = new Queue<Vector2I>();
        frontier.Enqueue(startCoord);
        Costs[startCoord.X, startCoord.Y] = 0;

        while (frontier.Count > 0)
        {
            var currCell = frontier.Dequeue();
            _visited[currCell.X, currCell.Y] = true;
            for (int rowShift = -1; rowShift <= 1; rowShift++)
            for (int colShift = -1; colShift <= 1; colShift++)
            {
                var adjCell = new Vector2I(currCell.X + rowShift, currCell.Y + colShift);

                if (rowShift == 0 && colShift == 0) continue; // skip currCell
                if (adjCell.Y < 0 || adjCell.Y >= _terrainRowLength ||
                    adjCell.X < 0 || adjCell.X >= _terrainColumnLength) continue; // out of bounds;
                if (_visited[adjCell.X, adjCell.Y]) continue; // already processed
                if (_terrainMatrix[adjCell.X, adjCell.Y] == _nonWalkableCellState) continue; // obstacle 

                _visited[adjCell.X, adjCell.Y] = true;
                Costs[adjCell.X, adjCell.Y] = Costs[currCell.X, currCell.Y] + 1;
                frontier.Enqueue(adjCell);
            }
        }
    }

    private void GenDirectionMatrix()
    {
        for (int col = 1; col < _terrainColumnLength-1; col++)
        for (int row = 1; row < _terrainRowLength-1; row++)
        {
            
            int xDir = Costs[col - 1, row+1] - Costs[col + 1, row-1];
            int yDir = Costs[col-1, row - 1] - Costs[col+1, row + 1];
            
            Vector2 finalDir =  new Vector2(xDir, yDir).Normalized();

            Directions[col, row] = finalDir;
            
            
            // int currMin = int.MaxValue;
            // var currDir = Vector2I.Zero;
            // for (int colShift = -1; colShift <= 1; colShift++)
            // for (int rowShift = -1; rowShift <= 1; rowShift++)
            // {
            //     if (colShift == 0 && rowShift == 0) continue;
            //     
            //     int currCol = colShift + col;
            //     int currRow = rowShift + row;
            //     
            //     if (currRow < 0 || currRow >= _terrainRowLength || currCol < 0 ||
            //         currCol >= _terrainColumnLength) continue;
            //     
            //     if (Costs[currCol, currRow] < currMin)
            //     {
            //         currDir = new Vector2I(colShift, rowShift);
            //         currMin = Costs[currCol, currRow];
            //     }
            // }
            //
            // Directions[col, row] = currDir;
        }
    }
}