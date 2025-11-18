using Godot;
using System;

public partial class CharacterSelect : Control
{
	public override void _Ready()
	{
		var rabbit = GetNode<AnimatedSprite2D>("Rabbit");
		var frog = GetNode<AnimatedSprite2D>("Frog");
		var raccoon = GetNode<AnimatedSprite2D>("Raccoon");
		var fox = GetNode<AnimatedSprite2D>("Fox");

		rabbit.Play("default");
		rabbit.Frame = 1;
		frog.Play("default");
		frog.Frame = 3;
		raccoon.Play("default");
		raccoon.Frame = 4;
		fox.Play("default");
	}

	public override void _Process(double delta)
	{
	}
}
