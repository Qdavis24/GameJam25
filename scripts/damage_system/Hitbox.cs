using Godot;
using System;
namespace GameJam25.scripts.damage_system;
// Hitbox.cs
using Godot;

public partial class Hitbox : Area2D
{
    [Export] public int Damage = 10;
    [Export] public int KnockbackWeight = 800;
}
