using Godot;
using System;
using System.Collections.Generic;


public partial class Testflowfield : Node2D
{
    [Export] public TileMapLayer tilemap;
    [Export] public PackedScene label;
    [Export] private int Size;
    [Export] public PackedScene Arrow;


    private int[,] _matrix;
    private int[,] _costs;
    private Vector2I[,] _directions;

    private bool[,] visited;
    private Queue<Coord> frontier;

    private List<Label> labels;

    public override void _Ready()
    {
        labels = new List<Label>();
        _matrix = new int[Size, Size];
        for (int i = 0; i < Size; i++)
        for (int j = 0; j < Size; j++)
        {
            _matrix[i, j] = GD.RandRange(0, 1);
            if (_matrix[i, j] == 1)
                tilemap.SetCell(new Vector2I(j, i), 0, new Vector2I(6, 0));
            else
            {
                tilemap.SetCell(new Vector2I(j, i), 0, new Vector2I(15, 0));
            }

            //CreateLabel(i, j, $"{i}, {j}");
        }

        //Init();
    }

    private void CreateLabel(int row, int col, string cost)
    {
        var l = label.Instantiate<Label>();
        l.Text = cost;
        l.Position = tilemap.MapToLocal(new Vector2I(col, row));
        AddChild(l);
        labels.Add(l);
    }

    private void CreateArrow(int row, int col, Vector2I direction)
    {
        var arrow = Arrow.Instantiate<Polygon2D>();
        var tilePos = tilemap.MapToLocal(new Vector2I(col, row));
        var nextTilePos =  tilemap.MapToLocal(new Vector2I(col + direction.Y, row + direction.X));
        arrow.Position = tilePos;
        AddChild(arrow);
        arrow.Rotation = (nextTilePos - tilePos).Angle();
    }

    private void Init()
    {
        _directions = new Vector2I[Size, Size];
        _costs = new int[Size, Size];
        visited = new bool[Size, Size];
        frontier = new Queue<Coord>();
        frontier.Enqueue((GD.RandRange(0, Size - 1), GD.RandRange(0, Size - 1)));
        foreach (var label1 in labels)
        {
            label1.QueueFree();
        }

        labels.Clear();
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("attack"))
        {
            for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
            {
                if (!visited[i, j]) continue;
                CreateArrow(i, j, _directions[i,j]);
                
                CreateLabel(i, j, $"{_costs[i,j]}");
                //CreateLabel(i, j, $"{_directions[i,j].X}, {_directions[i,j].Y}");
            }
        }

        if (Input.IsActionJustPressed("regen"))
        {
            Init();
            while (frontier.Count > 0)
            {
                var tile = frontier.Dequeue();
                visited[tile.row, tile.col] = true;
                for (int row = -1; row <= 1; row++)
                for (int col = -1; col <= 1; col++)
                {
                    Coord currTile = (tile.row + row, tile.col + col);
                    if ((row == 0 && col == 0) || currTile.row < 0 || currTile.row >= Size ||
                        currTile.col < 0 || currTile.col >= Size || visited[currTile.row, currTile.col] ||
                        _matrix[currTile.row, currTile.col] == 0)
                    {
                        continue;
                    }

                    visited[currTile.row, currTile.col] = true;
                    _costs[currTile.row, currTile.col] = _costs[tile.row, tile.col] + 1;

                    //tilemap.SetCell(new Vector2I(currTile.col, currTile.row), 0,
                       // new Vector2I(3, 0)); // neighbors are dirt
                    //CreateLabel(currTile.row, currTile.col, $"{_costs[currTile.row, currTile.col]}");
                    frontier.Enqueue(currTile);
                }
            }
            
            for(int i =0; i < Size; i++)
            for (int j = 0; j < Size; j++)
            {
                int min = _costs[i, j];
                var dir = Vector2I.Zero;
                
                for (int row=-1; row <= 1; row++)
                for (int col = -1; col <= 1; col++)
                {
                    if (row == 0 && col == 0) continue;
                    int currRow = row + i;
                    int currCol = col + j;
                    if (currRow < 0 || currRow >= Size || currCol < 0 || currCol >= Size) continue;
                    if (!visited[currRow, currCol]) continue;
                    if (_costs[currRow, currCol] < min)
                    {
                        dir = new Vector2I(row, col);
                        min = _costs[currRow, currCol];
                    }
                }
                
                _directions[i, j] = dir;
            }

            GD.Print("Done");
        }


        // if (frontier.Count > 0)
        // {
        //     var newFrontier = new Queue<Vector2I>() < Vector2I > ();
        //
        //     foreach (Vector2I coord in frontier) // set their visited and coord
        //     {
        //         // var l = label.Instantiate<Label>();
        //         // l.Text = $"cost: {currCost}";
        //         // l.Position = tilemap.MapToLocal(new Vector2I(coord.Y, coord.X));
        //         // AddChild(l);
        //         // labels.Add(l);
        //         //visited[coord.X, coord.Y] = true;
        //         _costs[coord.X, coord.Y] = currCost;
        //         //tilemap.SetCell(new Vector2I(coord.Y, coord.X), 0, new Vector2I(13, 0)); // visited is stone
        //     }
        //
        //     foreach (var node in frontier) // find their neighbors
        //     {
        //         for (int row = -1; row <= 1; row++)
        //         for (int col = -1; col <= 1; col++)
        //         {
        //             int currRow = row + node.X;
        //             int currCol = col + node.Y;
        //             if ((row == 0 && col == 0) || currRow < 0 || currRow >= _matrix.GetLength(0) || currCol < 0 ||
        //                 currCol >= _matrix.GetLength(1))
        //             {
        //                 continue;
        //             }
        //
        //             if (visited[currRow, currCol])
        //             {
        //                 continue;
        //             }
        //
        //             if (_matrix[currRow, currCol] == 0)
        //             {
        //                 continue;
        //             }
        //
        //
        //             visited[currRow, currCol] = true;
        //             GD.Print(new Vector2I(currRow, currCol));
        //             newFrontier.Add(new Vector2I(currRow, currCol));
        //             //tilemap.SetCell(new Vector2I(currCol, currRow), 0, new Vector2I(3, 0)); // neighbors are dirt
        //         }
        //     }
        //
        //     frontier = new List<Vector2I>();
        //     frontier = newFrontier;
        //     currCost++;
        // }
    }
}