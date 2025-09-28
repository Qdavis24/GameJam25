using Godot;
using System;

public partial class SlashAttackNode2d : Node2D
{
	public Vector2 Direction { get; set; } = Vector2.Right;

	[Export] public float Lifetime = 0.2f;

	private AnimatedSprite2D _anim;

	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// Rotate to match attack direction
		Rotation = Mathf.Atan2(Direction.Y, Direction.X);

		// Play the slash animation
		_anim.Play("slash");

		// Auto-destroy when animation finishes
		_anim.AnimationFinished += () => QueueFree();

		// Safety timer if animation resource doesn’t trigger
		GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
	}
}
