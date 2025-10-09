using System;
using Godot;
using System.Collections.Generic;

public partial class ChaseState : EnemyState
{
    private double _timePerCycle;
    private bool _targetInRange;

    private float _chasePathMagnitude;
    private double _currTimeCycle;

    private Vector2[] _steeringVectors = new Vector2[3]; // will be obstacles, enemies, player
    private float[] _steeringWeights = { 0.8f, 0.2f, 0.7f }; // obstacles, ENEMIES, player

    private List<Node2D> _obstaclesInRange = new List<Node2D>();
    private List<Node2D> _enemiesInRange = new List<Node2D>();

    public override void Enter()
    {
        Owner.Speed = GD.RandRange(100, 200);
        _chasePathMagnitude = GD.Randf()*2;
        _timePerCycle = GD.Randf() * 2f + 2;
        _currTimeCycle = _timePerCycle - (GD.Randf() * _timePerCycle);
        Owner.animations.Play("Move");
    }

    public override void Exit()
    {
        Owner.animations.Stop();
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 obstaclesDirection = GetObsticleDir();
        Vector2 enemiesDirection = GetEnemiesDir();
        Vector2 playerDirection = (Owner.CurrentTarget.GlobalPosition - Owner.GlobalPosition).Normalized();
        _steeringVectors[0] = obstaclesDirection;
        _steeringVectors[1] = enemiesDirection;
        _steeringVectors[2] = playerDirection;
        Vector2 baseDir = GetCurrentDir();
        Vector2 perpDirection = new Vector2(-baseDir.Y, baseDir.X);
        float sample = Owner.ChasePath.Sample((float)(_currTimeCycle / _timePerCycle));
        Vector2 interpolatedDir =
            (baseDir + (perpDirection * sample * _chasePathMagnitude)).Normalized();
        _currTimeCycle += delta;
        Owner.Velocity = interpolatedDir * Owner.Speed;

        if (_currTimeCycle >= _timePerCycle)
        {
            _currTimeCycle = 0;
        }
    }

    private Vector2 GetCurrentDir()
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
            Vector2 diff = (Owner.GlobalPosition - currObstacle.GlobalPosition);
            float distance = diff.Length();
            if(distance > 0) sumVector += diff.Normalized() / distance;
        }

        return sumVector.Normalized();
    }

    private Vector2 GetEnemiesDir()
    {
        if (_enemiesInRange.Count == 0) return Vector2.Zero;

        Vector2 sumVector = Vector2.Zero;
        
        foreach (Node2D currEnemy in _enemiesInRange)
        {
            Vector2 diff = (Owner.GlobalPosition - currEnemy.GlobalPosition);
            float distance = diff.Length();
            if(distance > 0) sumVector += diff.Normalized() / distance;
        }

        return sumVector.Normalized();
    }

    public void OnAggroRangeExited(Node2D body)
    {
        if (body == Owner.CurrentTarget)
        {
            EmitSignal(SignalName.StateTransition, Name, "IdleState");
        }
    }

    private void PrintEnemies()
    {
        String ls = Owner.Name + " current enemies : [";
        foreach (Node2D enemy in _enemiesInRange)
        {
            ls += enemy.Name + ", ";
        }

        ls += "]";
        GD.Print(ls);
    }
    public void OnSteeringRangeEntered(Node2D body)
    {
        PrintEnemies();
        if (body is TileMapLayer && body.Name == "ObstacleLayer")
        {
            _obstaclesInRange.Add(body);
        }
        else if (body.IsInGroup("Enemies"))
        {
            _enemiesInRange.Add(body);
        }
    }

    public void OnSteeringRangeExited(Node2D body)
    {
        PrintEnemies();
        if (body is TileMapLayer && body.Name == "ObstacleLayer")
        {
            _obstaclesInRange.Remove(body);
        }
        else if (body.IsInGroup("Enemies"))
        {
            _enemiesInRange.Remove(body);
        }
    }
}