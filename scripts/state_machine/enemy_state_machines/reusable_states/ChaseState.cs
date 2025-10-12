using System;
using Godot;
using System.Collections.Generic;
using GameJam25.scripts.state_machine;
using GameJam25.scripts.state_machine.enemy_state_machines;

public partial class ChaseState : EState
{
    [Export] private float _minPathMag;
    [Export] private float _maxPathMag;
    [Export] private Curve _path;
    [Export] private float[] _steeringWeights; // obstacles, ENEMIES, player
    [Export] private int _numRaycastSamples;
    [Export] int _sampleDistance;

    private Vector2[] _sampleDirections;


    [Export] private double _minTimePerCycle;
    [Export] private double _maxTimePerCycle;

    private double _timePerCycle;


    private double _currTimeCycle;
    private Vector2[] _steeringVectors = new Vector2[3]; // will be obstacles, enemies, player

    private List<Node2D> _obstaclesInRange = new List<Node2D>();
    private List<Node2D> _enemiesInRange = new List<Node2D>();

    private void CalculateSampleDirections()
    {
        float radIncr = (2 * Mathf.Pi) / _numRaycastSamples;
        float currRad = 0;
        for (int i = 0; i < _numRaycastSamples; i++)
        {
            _sampleDirections[i] = Vector2.FromAngle(currRad);
            currRad += radIncr;
        }
    }

    public override void Enter()
    {
        _sampleDirections = new Vector2[_numRaycastSamples];
        CalculateSampleDirections();
        _stateMachine.Owner.Speed += GD.RandRange(0, 50);
        _minPathMag += _minPathMag + _maxPathMag * GD.Randf();
        _timePerCycle = _minTimePerCycle + _maxTimePerCycle * GD.Randf();
        _currTimeCycle = _timePerCycle - (GD.Randf() * _timePerCycle);
        _stateMachine.Owner.Animations.Play("Move");
    }

    public override void Exit()
    {
        _stateMachine.Owner.Animations.Stop();
    }


    public override void PhysicsUpdate(double delta)
    {
        if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(_stateMachine.InstanceContext.CurrentTarget
                .GlobalPosition) <
            _stateMachine.Owner.AttackRange)
        {
            _stateMachine.TransitionTo("ExplodeState");
        }

        Vector2 obstaclesDirection = GetObsticleDir();
        Vector2 enemiesDirection = GetEnemiesDir();
        Vector2 playerDirection =
            (_stateMachine.InstanceContext.CurrentTarget.GlobalPosition - _stateMachine.Owner.GlobalPosition)
            .Normalized();

        _steeringVectors[0] = obstaclesDirection;
        _steeringVectors[1] = enemiesDirection;
        _steeringVectors[2] = playerDirection;

        Vector2 desiredDir = GetMostDesirableDir();
        Vector2 baseDir = desiredDir;
        if (_currTimeCycle > _timePerCycle)
        {
            _currTimeCycle = 0;
            var currValidDirs = GetValidDirs();
            baseDir = GetBaseDir(currValidDirs, desiredDir);
        }

        Vector2 perpDirection = new Vector2(-baseDir.Y, baseDir.X);
        GD.Print(baseDir);

        float sample = _path.Sample((float)(_currTimeCycle / _timePerCycle));
        Vector2 interpolatedDir =
            (baseDir + (perpDirection * sample * _minPathMag)).Normalized();
        _currTimeCycle += delta;
        _stateMachine.Owner.Velocity = interpolatedDir * _stateMachine.Owner.Speed;
        
        _stateMachine.Owner.MoveAndSlide();
    }

    private Vector2 GetBaseDir(List<Vector2> validDirs, Vector2 desiredDir)
    {
        float maxDotP = -Mathf.Inf;
        Vector2 baseDir = validDirs[0];

        foreach (Vector2 dir in validDirs)
        {
            float currDotP = dir.Dot(desiredDir);
            if (currDotP > maxDotP)
            {
                baseDir = dir;
                maxDotP = currDotP;
            }
        }

        return baseDir;
    }

    private Vector2 GetMostDesirableDir()
    {
        Vector2 sumVector = Vector2.Zero;
        for (int i = 0; i < _steeringVectors.Length; i++)
        {
            sumVector += _steeringVectors[i] * _steeringWeights[i];
        }

        return sumVector.Normalized();
    }

    private Vector2 GetObsticleDir()

    {
        if (_obstaclesInRange.Count == 0) return Vector2.Zero;

        Vector2 sumVector = Vector2.Zero;
        foreach (Node2D currObstacle in _obstaclesInRange)
        {
            Vector2 diff = (_stateMachine.Owner.GlobalPosition - currObstacle.GlobalPosition);
            float distance = diff.Length();
            if (distance > 0) sumVector += diff.Normalized() / distance;
        }

        return sumVector.Normalized();
    }

    private Vector2 GetEnemiesDir()
    {
        if (_enemiesInRange.Count == 0) return Vector2.Zero;

        Vector2 sumVector = Vector2.Zero;

        foreach (Node2D currEnemy in _enemiesInRange)
        {
            Vector2 diff = (_stateMachine.Owner.GlobalPosition - currEnemy.GlobalPosition);
            float distance = diff.Length();
            if (distance > 0) sumVector += diff.Normalized() / distance;
        }

        return sumVector.Normalized();
    }

    private List<Vector2> GetValidDirs()
    {
        List<Vector2> validDirIndexes = new List<Vector2>();
        for (int i = 0; i < _numRaycastSamples; i++)
        {
            Vector2 dir = _sampleDirections[i];
            var spaceState = GetWorld2D().DirectSpaceState;
            // use global coordinates, not local to node
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + dir * _sampleDistance);
            query.CollisionMask = 2;
            var result = spaceState.IntersectRay(query);
            if (result.Count > 0 && result["collider"].As<Node2D>().IsInGroup("ObstacleLayer")) continue;
            validDirIndexes.Add(dir);
        }

        return validDirIndexes;
    }

    private void OnAggroRangeExited(Node2D body)
    {
        if (body != _stateMachine.InstanceContext.CurrentTarget) return;

        _stateMachine.InstanceContext.CurrentTarget = null;
        _stateMachine.TransitionTo("IdleState");
    }

    private void OnSteeringRangeEntered(Node2D body)
    {
        //DebugPrintEnemiesInSteeringRange();
        if (body is TileMapLayer && body.Name == "ObstacleLayer")
        {
            _obstaclesInRange.Add(body);
        }
        else if (body.IsInGroup("Enemies"))
        {
            _enemiesInRange.Add(body);
        }
    }

    private void OnSteeringRangeExited(Node2D body)
    {
        //DebugPrintEnemiesInSteeringRange();
        if (body is TileMapLayer && body.Name == "ObstacleLayer")
        {
            _obstaclesInRange.Remove(body);
        }
        else if (body.IsInGroup("Enemies"))
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