using Godot;

public partial class IdleState : EnemyState
{
    [Signal]
    public delegate void StateTransitionEventHandler(string prevState, string nextState);
    private bool _targetInRange;
    
    public override void Enter()
    {
        _targetInRange = false;
        Owner.animations.Play("Idle");
    }

    public override void Exit()
    {
        Owner.animations.Stop();
    }

    public override void Update(double delta)
    {
        if (_targetInRange)
        {
            EmitSignal(SignalName.StateTransition, Name, "ChaseState");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        
    }

    void OnBodyEntered(Node2D body)
    {
        foreach (string group in Owner.AggroGroups)
        {
            if (body.IsInGroup(group)) _targetInRange = true;
        }
    }
    
}