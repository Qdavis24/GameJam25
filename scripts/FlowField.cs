using System.Collections.Generic;
using System.Data;
using Godot;

public class FlowField
{
    public Vector2I[,] Directions;

    private int[,] _terrainMatrix;
    private int _walkableCellState;
    private int _terrainRowLength;
    private int _terrainColumnLength;


    public FlowField(int[,] terrainMatrix, int walkableCellState)
    {
        _terrainMatrix = terrainMatrix;
        _walkableCellState = walkableCellState;
        _terrainRowLength = _terrainMatrix.GetLength(0);
        _terrainColumnLength = _terrainMatrix.GetLength(1);
    }

    public void GenerateFlowFieldFrom(Coord coord)
    {
        Directions = new Vector2I[_terrainRowLength, _terrainColumnLength];
        var costs = GenCostMatrix(coord);
        Directions = GenDirectionMatrix(costs);
    }

    private int[,] GenCostMatrix(Coord startCoord)
    {
        var visited = new bool[_terrainRowLength, _terrainColumnLength];
        var costs = new int[_terrainRowLength, _terrainColumnLength];
        var frontier = new Queue<Coord>();
        frontier.Enqueue(startCoord);

        while (frontier.Count > 0)
        {
            var currCell = frontier.Dequeue();
            visited[currCell.row, currCell.col] = true;
            for (int rowShift = -1; rowShift <= 1; rowShift++)
            for (int colShift = -1; colShift <= 1; colShift++)
            {
                Coord adjCell = (currCell.row + rowShift, currCell.col + colShift);
                if ((rowShift == 0 && colShift == 0) || adjCell.row < 0 || adjCell.row >= _terrainRowLength ||
                    adjCell.col < 0 || adjCell.col >= _terrainColumnLength || visited[adjCell.row, adjCell.col] ||
                    _terrainMatrix[adjCell.row, adjCell.col] != _walkableCellState)
                {
                    continue;
                }

                visited[adjCell.row, adjCell.col] = true;
                costs[adjCell.row, adjCell.col] = costs[currCell.row, currCell.col] + 1;
                frontier.Enqueue(adjCell);
            }
        }

        return costs;
    }

    private Vector2I[,] GenDirectionMatrix(int[,] costs)
    {
        var directions = new Vector2I[_terrainRowLength, _terrainColumnLength];
        for (int i = 0; i < _terrainRowLength; i++)
        for (int j = 0; j < _terrainColumnLength; j++)
        {
            int min = costs[i, j];
            var dir = Vector2I.Zero;

            for (int row = -1; row <= 1; row++)
            for (int col = -1; col <= 1; col++)
            {
                if (row == 0 && col == 0) continue;
                int currRow = row + i;
                int currCol = col + j;
                if (currRow < 0 || currRow >= _terrainRowLength || currCol < 0 ||
                    currCol >= _terrainColumnLength) continue;
                if (costs[i, j] == 0) continue;
                if (costs[currRow, currCol] < min)
                {
                    dir = new Vector2I(row, col);
                    min = costs[currRow, currCol];
                }
            }

            directions[i, j] = dir;
        }


        GD.Print("Done");
        return directions;
    }
}