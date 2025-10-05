using Godot;

public partial class ChaseState : EnemyState
{
    [Signal]
    public delegate void StateTransitionEventHandler(string prevState, string nextState);

    private bool _targetInRange;
    private float _chasePathMagnitude;
    private double _timePerCycle;
    private double _currTimeCycle;

    public override void Enter()
    {
        _chasePathMagnitude = GD.RandRange(0, 7);
        _timePerCycle = GD.RandRange(5, 15);
        _currTimeCycle = _timePerCycle;
        Owner.animations.Play("Move");
    }

    public override void Exit()
    {
        Owner.animations.Stop();
        Owner.CurrentTarget = null;
    }

    public override void Update(double delta)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 direction = (Owner.CurrentTarget.GlobalPosition - Owner.GlobalPosition).Normalized();
        Vector2 perpDirection = new Vector2(-direction.Y, direction.X);
        float sample = Owner.ChasePath.Sample((float)(_currTimeCycle / _timePerCycle));
        Vector2 currDir =
            (direction + (perpDirection * sample * _chasePathMagnitude)).Normalized();
        _currTimeCycle += delta;
        Owner.Velocity = currDir * Owner.Speed;
        if (_currTimeCycle >= _timePerCycle)
        {
            _currTimeCycle = 0;
        }
    }

    public void OnBodyExited(Node2D body)
    {
        if (body == Owner.CurrentTarget)
        {
            EmitSignal(SignalName.StateTransition, Name, "IdleState");
        }
    }
}