using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

public partial class IdleState : EState
{
    private Vector2 _wanderDirection;
    private float _wanderTimer;
    
    public override void Enter()
    {
        _stateMachine.Owner.Animations.Play("Idle");
        PickNewDirection();
    }

    public override void Exit()
    {
        _stateMachine.Owner.Animations.Stop();
        _stateMachine.Owner.Velocity = Vector2.Zero;
    }

    public override void Update(double delta)
    {
        if (_stateMachine.InstanceContext.CurrentTarget != null)
        {
            _stateMachine.TransitionTo("ChaseState");
        }
        
        // Change direction every 2 seconds
        _wanderTimer -= (float)delta;
        if (_wanderTimer <= 0)
        {
            PickNewDirection();
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        _stateMachine.Owner.Velocity = _wanderDirection * 50f; // wander speed
        _stateMachine.Owner.MoveAndSlide();
    }
    
    private void PickNewDirection()
    {
        float angle = GD.Randf() * Mathf.Tau;
        _wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        _wanderTimer = 4f;
    }

    void OnAggroRangeEntered(Node2D body)
    {
        foreach (string group in _stateMachine.Owner.AggroGroups)
        {
            if (body.IsInGroup(group))
            {
                dynamic currTarget = null;
                if (group == "Players")
                {
                    currTarget = (Player)body;
                }
                
                if (currTarget == null)
                {
                    GD.PrintErr(
                        $"Null Current Target detected in AggroRange : Owner - {_stateMachine.Owner.Name} State - Enemy Idle State \n Make sure that Type {group} is added to IdleState OnAggroRangeEntered Callback");
                }
                _stateMachine.InstanceContext.CurrentTarget = currTarget;
                _stateMachine.TransitionTo("ChaseState");
            }
        }
    }
}