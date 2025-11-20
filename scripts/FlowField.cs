using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public partial class FlowField : Node2D
{
    [Export] private float _updateDistance = 100f;
    [Export] private int _cellsProcessedPerFrame = 500;

    public Vector2[,] _directions;

    private int[,] _costs;
    private bool[,] _visited;
    private Queue<Vector2I> _frontier;

    private TileMapLayer _tileMapLayer;
    private int[,] _terrainMatrix;
    private int _nonWalkableCellState;
    private int _terrainRowLength;
    private int _terrainColumnLength;

    private Node2D _target;
    private Vector2 _lastUpdatePos;

    private bool _active;
    private bool _updating;
    public bool Valid;


    public void Init(int[,] terrainMatrix, int nonWalkableCellState, TileMapLayer tileMapLayer, Node2D target)
    {
        _target = target;
        _terrainMatrix = terrainMatrix;
        _nonWalkableCellState = nonWalkableCellState;
        _tileMapLayer = tileMapLayer;

        _terrainColumnLength = _terrainMatrix.GetLength(0);
        _terrainRowLength = _terrainMatrix.GetLength(1);
        
        _lastUpdatePos = target.GlobalPosition;

        _frontier = new Queue<Vector2I>();
        _directions = new Vector2[_terrainColumnLength, _terrainRowLength];
        
        _active = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active) return;
        if (!_updating || (_lastUpdatePos - _target.GlobalPosition).Length() >= _updateDistance)
        {
            _updating = true;
            _lastUpdatePos = _target.GlobalPosition;
            InitNewField();
        }
        
    }

    public override void _Process(double delta)
    {
        if (_updating)
        {
            int count = 0;
            while (count < _cellsProcessedPerFrame)
            {
                count++;
                if (_frontier.Count == 0)
                {
                    Valid = true;
                    _updating = false;
                    return;
                }
                var currCell = _frontier.Dequeue();
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
                    _costs[adjCell.X, adjCell.Y] = _costs[currCell.X, currCell.Y] + 1;
                    _frontier.Enqueue(adjCell);
                }

                _directions[currCell.X, currCell.Y] = CalculateCellDirection(currCell);
            }
        }
    }

    private void InitNewField()
    {
        _frontier.Clear();
        _visited = new bool[_terrainColumnLength, _terrainRowLength];
        _costs = new int[_terrainColumnLength, _terrainRowLength];

        for (int col = 0; col < _terrainColumnLength; col++)
        for (int row = 0; row < _terrainRowLength; row++)
            _costs[col, row] = int.MaxValue;
        var startingCoord = GetTargetCoord(_target.GlobalPosition);
        _costs[startingCoord.X, startingCoord.Y] = 0;
        _frontier.Enqueue(startingCoord);
    }

    private Vector2 CalculateCellDirection(Vector2I currCell)
    {
        if (currCell.X < 1 || currCell.Y < 1 || currCell.X > _terrainColumnLength - 2 ||
            currCell.Y > _terrainRowLength - 2) return Vector2.Zero;
            
        int xDir = _costs[currCell.X - 1, currCell.Y + 1] - _costs[currCell.X + 1, currCell.Y - 1];
        int yDir = _costs[currCell.X - 1, currCell.Y - 1] - _costs[currCell.X + 1, currCell.Y + 1];

        Vector2 finalDir = new Vector2(xDir, yDir).Normalized();

        return finalDir;
    }

    private Vector2I GetTargetCoord(Vector2 globalPosition)
    {
        return _tileMapLayer.LocalToMap(_tileMapLayer.ToLocal(globalPosition));
    }

    public Vector2 GetDirection(Vector2 globalPosition)
    {
        var dir = Vector2.Zero;
        var numSampleDirs = 0;
        var sampleCoord = GetTargetCoord(globalPosition);
        for (int colShift = -1; colShift <= 1; colShift++)
        for (int rowShift = -1; rowShift <= 1; rowShift++)
        {
            var currCol = sampleCoord.X + colShift;
            var currRow = sampleCoord.Y + rowShift;
            if (currCol < 0 || currCol >= _terrainColumnLength || currRow < 0 || currRow >= _terrainRowLength) continue;
            var currDir = _directions[currCol, currRow];
            if (currDir == Vector2.Zero) continue;
            dir += currDir;
            numSampleDirs++;
        }
        dir /= numSampleDirs;
        return dir.Normalized();
    }
}