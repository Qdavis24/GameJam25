using Godot;
using System;
namespace GameJam25.scripts.player_package.hitbox;
// Hitbox.cs
using Godot;

public partial class Hitbox : Area2D
{
    [Export] public int Damage = 10;
    
    public override void _Ready()
    {
        AddToGroup("PlayerAttacks");
    }
}
