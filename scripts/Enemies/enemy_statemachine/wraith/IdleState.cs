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
 
        
    }

    public override void PhysicsUpdate(double delta)
    {
        
    }

    void OnBodyEntered(Node2D body)
    {
        GD.Print(body.Name);
        foreach (string group in Owner.AggroGroups)
        {

            if (body.IsInGroup(group))
            {
                
                Owner.CurrentTarget = body;
                EmitSignal(SignalName.StateTransition, Name, "ChaseState");
            }
        }
    }
    
}