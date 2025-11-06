using System;
using Godot;
using System.Collections.Generic;
using GameJam25.scripts.state_machine.enemy_state_machines;

public partial class ChaseState : EState
{
    [Export] private float _minPathMag;
    [Export] private float _maxPathMag;
    [Export] private Curve _path;
    [Export] private float[] _steeringWeights; // obstacles, ENEMIES, player
    [Export] private int _numRaycastSamples;
    [Export] int _sampleDistance;


    [Export] private double _minTimePerCycle;
    [Export] private double _maxTimePerCycle;

    private double _timePerCycle;
    private double _currTimeCycle;

    private List<Node2D> _enemiesInRange = new List<Node2D>();

    private float _togglePathfindingRange = 300.0f; // range to stop using FF to pathfind 

    private enum Pathfinding
    {
        FlowField,
        Traditional
    }

    private Pathfinding _currPathfindingMode = Pathfinding.FlowField;

    public override void Enter()
    {
        _stateMachine.Owner.Speed += GD.RandRange(0, 50);
        _minPathMag += _minPathMag + _maxPathMag * GD.Randf();
        _timePerCycle = _minTimePerCycle + _maxTimePerCycle * GD.Randf();
        _currTimeCycle = _timePerCycle - (GD.Randf() * _timePerCycle);
    }

    public override void Exit()
    {
        _stateMachine.Owner.Animations.Stop();
    }


    public override void PhysicsUpdate(double delta)
    {
        if (GameManager.Instance.Player == null) return;
        if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(GameManager.Instance.Player.GlobalPosition) <
            _stateMachine.Owner.AttackRange)
        {
            _stateMachine.TransitionTo("ExplodeState");
        }

        if (GameManager.Instance.CurrFlowField.Directions == null) return;
        if ((_stateMachine.Owner.GlobalPosition - GameManager.Instance.Player.GlobalPosition).LengthSquared() <
            _togglePathfindingRange * _togglePathfindingRange) _currPathfindingMode = Pathfinding.Traditional;
        else _currPathfindingMode = Pathfinding.FlowField;
        
        var baseDir = _currPathfindingMode == Pathfinding.FlowField ? GetFlowFieldBaseDir() : GetTradBaseDir();
        baseDir = (baseDir + GetEnemiesDir() * .8f) / 2;

        Vector2 perpDirection = new Vector2(-baseDir.Y, baseDir.X);


        float sample = _path.Sample((float)(_currTimeCycle / _timePerCycle));
        Vector2 interpolatedDir =
            (baseDir + (perpDirection * sample * _minPathMag)).Normalized();
        _currTimeCycle += delta;
        _stateMachine.Owner.Velocity = interpolatedDir * _stateMachine.Owner.Speed;

        _stateMachine.Owner.MoveAndSlide();
    }

    private Vector2 GetTradBaseDir()
    {
        return (GameManager.Instance.Player.GlobalPosition - _stateMachine.Owner.GlobalPosition).Normalized();
    }

    private Vector2 GetFlowFieldBaseDir()
    {
        var enemyCoord = GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer.LocalToMap(
            GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer.ToLocal(_stateMachine.Owner.GlobalPosition));
        var baseDir = Vector2.Zero;
        var flowFieldCols = GameManager.Instance.CurrFlowField.Directions.GetLength(0);
        var flowFieldRows = GameManager.Instance.CurrFlowField.Directions.GetLength(1);
        var numSampleDirs = 0;
        for (int colShift = -1; colShift <= 1; colShift++)
        for (int rowShift = -1; rowShift <= 1; rowShift++)
        {
            var currCol = enemyCoord.X + colShift;
            var currRow = enemyCoord.Y + rowShift;
            if (currCol < 0 || currCol >= flowFieldCols || currRow < 0 || currRow >= flowFieldRows) continue;
            var currDir = GameManager.Instance.CurrFlowField.Directions[currCol, currRow];
            if (currDir == Vector2.Zero) continue;
            baseDir += currDir;
            numSampleDirs++;
        }

        baseDir /= numSampleDirs;
        return baseDir;
    }

    private Vector2 GetEnemiesDir()
    {
        if (_enemiesInRange.Count == 0) return Vector2.Zero;

        Vector2 sumVector = Vector2.Zero;

        foreach (Node2D currEnemy in _enemiesInRange)
        {
            Vector2 diff = (_stateMachine.Owner.GlobalPosition - currEnemy.GlobalPosition);
            sumVector += diff.Normalized();
        }

        return (sumVector / _enemiesInRange.Count).Normalized();
    }

    private void OnSteeringRangeEntered(Node2D body)
    {
        if (body.IsInGroup("Enemies"))
        {
            _enemiesInRange.Add(body);
        }
    }

    private void OnSteeringRangeExited(Node2D body)
    {
        if (body.IsInGroup("Enemies"))
        {
            _enemiesInRange.Remove(body);
        }
    }

    private void DebugPrintEnemiesInSteeringRange()
    {
        String ls = Owner.Name + " current enemies : [";
        foreach (Node2D enemy in _enemiesInRange)
        {
            ls += enemy.Name + ", ";
        }

        ls += "]";
        GD.Print(ls);
    }
}