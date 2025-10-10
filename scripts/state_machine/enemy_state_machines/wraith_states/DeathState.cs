using GameJam25.scripts.state_machine.enemy_state_machines;

namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class DeathState : EState
{
    public override void Enter()
    {
        _stateMachine.Owner.Animations.Play("Die");
        _stateMachine.Owner.DeathParticles.Emitting = true;
    }
    
    public override void Update(double delta)
    {
        if (!_stateMachine.Owner.Animations.IsPlaying())
            Owner.QueueFree();
    }

}