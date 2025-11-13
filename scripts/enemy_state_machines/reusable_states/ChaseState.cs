using System;
using Godot;
using System.Collections.Generic;
using GameJam25.scripts.enemy_state_machines.base_classes;

namespace GameJam25.scripts.enemy_state_machines.reusable_states;

public partial class ChaseState : EState
{
    [ExportCategory("Interpolation Behavior")] 
    [Export] private bool Interpolate;
    [Export] private double _minDistancePerCycle;
    [Export] private double _maxDistancePerCycle;
    [Export] private float _minPathMag;
    [Export] private float _maxPathMag;
    [Export] private Curve _path;

    [ExportCategory("Boids Behavior")] [Export]
    private float _separationForce = 1.0f;

    [Export] private float _alignmentForce = .8f;
    [Export] private float _cohesionForce = .7f;

    [ExportCategory("General Behavior")] [Export]
    private float _boidsInfluence;

    [Export] private float _flowFieldInfluence;
    [Export] private float _minSpeed;
    [Export] private float _maxSpeed;
    [Export] private string _attackState;
    [Export] private float _togglePathfindingRange = 300.0f; // range to stop using FF to pathfind (pxls)

    private float _speed;
    private float _pathMagnitude;
    private double _distancePerCycle;
    private double _currDistance;

    private List<Enemy> _sameGroupEnemies;
 

    private enum Pathfinding
    {
        FlowField,
        Traditional
    }

    private Pathfinding _currPathfindingMode = Pathfinding.FlowField;

    // Super's abstract methods below
    public override void Enter()
    {
        _sameGroupEnemies = new List<Enemy>();
        _speed = _minSpeed + (_maxSpeed - _minSpeed) * GD.Randf();
        _pathMagnitude = _minPathMag + (_maxPathMag - _minPathMag) * GD.Randf();
        _distancePerCycle = (_minDistancePerCycle + (_maxDistancePerCycle - _minDistancePerCycle) * GD.Randf()) *
                            190.0f; // try to account for tile size
        _currDistance = 0.0f;
    }

    public override void Exit()
    {
        _stateMachine.Owner.Animations.Stop();
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        if (GameManager.Instance.Player == null) return;
        if (GameManager.Instance.CurrFlowField.Directions == null) return;

        if (_stateMachine.Owner.GlobalPosition.DistanceSquaredTo(GameManager.Instance.Player.GlobalPosition) <
            _stateMachine.Owner.AttackRange * _stateMachine.Owner.AttackRange) // transition to attack
        {
            _stateMachine.TransitionTo(_attackState);
        }


        if ((_stateMachine.Owner.GlobalPosition - GameManager.Instance.Player.GlobalPosition).LengthSquared() <
            _togglePathfindingRange * _togglePathfindingRange) _currPathfindingMode = Pathfinding.Traditional;
        else _currPathfindingMode = Pathfinding.FlowField; // determine pathfinding mode

        var baseDir = Vector2.Zero;

        switch (_currPathfindingMode)
        {
            case Pathfinding.Traditional:
                baseDir = GetTradDir();
                break;
            case Pathfinding.FlowField:
                baseDir = GetFlowFieldDir();
                break;
        }

        var boidsDir = GetBoidsDir();
        baseDir = (baseDir * _flowFieldInfluence + boidsDir * _boidsInfluence).Normalized();
        if (Interpolate) baseDir = GetInterpDir(baseDir);
        
        _stateMachine.Owner.Velocity = baseDir * _speed;
        _currDistance += _stateMachine.Owner.Velocity.Length();
        _stateMachine.Owner.MoveAndSlide();
    }


    // Helpers below
    private Vector2 GetInterpDir(Vector2 baseDir)
    {
        if (_currDistance >= _distancePerCycle) _currDistance = 0.0f;
        Vector2 perpDirection = new Vector2(-baseDir.Y, baseDir.X);
        float sample = _path.Sample((float)(_currDistance / _distancePerCycle));
        Vector2 interpolatedDir =
            (baseDir + (perpDirection * sample * _minPathMag)).Normalized();
        return interpolatedDir;
    }

    private Vector2 GetTradDir()
    {
        return (GameManager.Instance.Player.GlobalPosition - _stateMachine.Owner.GlobalPosition).Normalized();
    }

    private Vector2 GetFlowFieldDir()
    {
        var enemyCoord = GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer.LocalToMap(
            GameManager.Instance.CurrWorld.PhysicalData.BaseTileMapLayer.ToLocal(_stateMachine.Owner.GlobalPosition));
        var dir = Vector2.Zero;
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
            dir += currDir;
            numSampleDirs++;
        }

        dir /= numSampleDirs;
        return dir.Normalized();
    }

    private Vector2 GetBoidsDir()
    {
        if (_sameGroupEnemies.Count == 0) return Vector2.Zero;

        var seperationVec = Vector2.Zero;
        var alignmentVec = Vector2.Zero;
        var cohesionPos = Vector2.Zero;

        foreach (Enemy currEnemy in _sameGroupEnemies)
        {
            seperationVec += (_stateMachine.Owner.GlobalPosition - currEnemy.GlobalPosition).Normalized();
            alignmentVec += currEnemy.Velocity;
            cohesionPos += currEnemy.GlobalPosition;
        }

        seperationVec = seperationVec.Normalized();
        alignmentVec = alignmentVec.Normalized();
        cohesionPos /= _sameGroupEnemies.Count;
        var cohesionVec = (cohesionPos - _stateMachine.Owner.GlobalPosition).Normalized();
        return ((seperationVec * _separationForce) + (alignmentVec * _alignmentForce) + (cohesionVec * _cohesionForce))
            .Normalized();
    }
    

    private void OnSteeringRangeEntered(Node2D body)
    {
        if (body.IsInGroup(_stateMachine.Owner.GetGroups()[0]))
        {
            _sameGroupEnemies.Add(body as Enemy);
        }
    }

    private void OnSteeringRangeExited(Node2D body)
    {
        if (body.IsInGroup(_stateMachine.Owner.GetGroups()[0]))
        {
            _sameGroupEnemies.Remove(body as Enemy);
        }
  
    }

    private void DebugPrintEnemiesInSteeringRange()
    {
        String ls = Owner.Name + " current enemies : [";
        foreach (Node2D enemy in _sameGroupEnemies)
        {
            ls += enemy.Name + ", ";
        }

        ls += "]";
        GD.Print(ls);
    }
}