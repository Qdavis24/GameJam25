using Godot;

public partial class ChaseState : EnemyState
{
  
    [Signal]
    public delegate void StateTransitionEventHandler(string prevState, string nextState);
    private bool _targetInRange;
    private Node2D _currTarget;
    private float _chasePathMagnitude;
    private double _timePerCycle;
    private double _currTimeCycle;

    public override void Enter()
    {
        _targetInRange = false;
        _currTarget = null;
        _chasePathMagnitude = GD.RandRange(0, 5);
        _currTimeCycle = _timePerCycle;
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
        if (_targetInRange)
        {
            Vector2 direction = (_currTarget.Position - Owner.Position).Normalized();
            Vector2 perpDirection = direction.Normalized();
            Vector2 currDir =
                (direction + perpDirection *
                    Owner.ChasePath.Sample((float)(_currTimeCycle / _timePerCycle) * _chasePathMagnitude)).Normalized();
            _currTimeCycle += delta;
            Owner.Position += currDir * Owner.Speed;
            if (_currTimeCycle >= _timePerCycle)
            {
                _currTimeCycle = 0;
            }
        }
    }

    public void OnBodyEntered(Node2D body)
    {
        foreach (string group in Owner.AggroGroups)
        {
            if (body.IsInGroup(group))
            {
                _currTarget = body;
                _targetInRange = true;
            }
        }
    }

    public void OnBodyExited(Node2D body)
    {
        if (body == _currTarget)
        {
            _currTarget = null;
            _targetInRange = false;
            EmitSignal(SignalName.StateTransition, Name, "IdleState");
        }
    }
}