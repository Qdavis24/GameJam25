using Godot;
using System;
namespace GameJam25.scripts.damage_system;
// Hitbox.cs
using Godot;

public partial class Hitbox : Area2D
{
	[Export] public float Damage = 10f;
	public float KnockbackWeight;

	public override void _Ready()
	{
		KnockbackWeight = Damage * 30f; // 3 pixels per frame per damage point
	}
}
