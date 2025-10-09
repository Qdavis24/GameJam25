namespace GameJam25.scripts.Enemies.enemy_statemachine.wraith;

public partial class DeathState : EnemyState
{
    public override void Enter()
    {
        Owner.animations.Play("Die");
        Owner.DeathParticles.Emitting = true;
    }

    public override void Exit()
    {
    }

    public override void Update(double delta)
    {
        if (!Owner.animations.IsPlaying())
            Owner.QueueFree();
    }

    public override void PhysicsUpdate(double delta)
    {
    }
}