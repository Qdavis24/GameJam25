using GameJam25.scripts.state_machine.enemy_state_machines;
using Godot;

public partial class IdleState : EState
{
    
    public override void Enter()
    {
        GD.Print(_stateMachine.Name);
        _stateMachine.Owner.Animations.Play("Idle");
    }

    public override void Exit()
    {
        _stateMachine.Owner.Animations.Stop();
    }

    public override void Update(double delta)
    {
        if (_stateMachine.InstanceContext.CurrentTarget != null)
        {
            _stateMachine.TransitionTo("ChaseState");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
    }

    void OnAggroRangeEntered(Node2D body)
        /*
         * this is a call back for the aggro range area 2D body entered
         * works to detect if player is in aggro range then store a ref to player in context for later state (chase)
         * also transitions state on detection
         *
         * ISSUES:
         * body entered doesn't happen again if player is already in body
         * IE if we transition to IDLE again while player is still inside area2D
         *
         * good spot to fix / add better logic for this
         */
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
                // ADD all groups here as we increase aggro groups!!
                // potential null ref if not added
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