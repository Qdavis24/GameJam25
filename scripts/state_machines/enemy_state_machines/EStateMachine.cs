using GameJam25.scripts.state_machine.enemy_states;
using Godot;

namespace GameJam25.scripts.state_machine;
/*
 * Contains the specific type for Owner, and Context
 *
 * REFER to State Machine for more documentation
 */
public partial class EStateMachine : StateMachine
{
    public Enemy Owner;
    public EInstanceContext InstanceContext;

    public override void _Ready()
    {
        Owner = (Enemy) GetParent();
        foreach (Node child in GetChildren())
        {
            if (child is EInstanceContext context)
            {
                InstanceContext = context;
            }
        }
        base._Ready();
        
        
    }
}