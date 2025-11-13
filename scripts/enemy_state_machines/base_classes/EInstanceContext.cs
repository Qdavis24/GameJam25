using Godot;

namespace GameJam25.scripts.enemy_state_machines.base_classes;

public partial class EInstanceContext : Node2D
{
    public Vector2 KnockbackDir = Vector2.Zero;
    public int KnockbackWeight = 0;
}