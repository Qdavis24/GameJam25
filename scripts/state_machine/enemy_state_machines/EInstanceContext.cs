using Godot;
using System;
using System.Collections.Generic;

namespace GameJam25.scripts.state_machine.enemy_states;

/*
 * This will act as a data bucket to keep information between state changes
 * Specifically for Data only a single instance of an enemy will have access to
 * (Potential EnemyGroupContext if we want group data bucket)
 *
 * EState Machine will have a reference of an instance of this class as this is the instance data bucket specifically for enemies
 *
 * EStateMachine -> EnemyInstanceContext;
 */
public partial class EInstanceContext : InstanceContext
{
    public Player CurrentTarget;
    public Vector2 KnockBackDir = Vector2.Zero;
}