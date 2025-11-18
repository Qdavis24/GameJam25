using Godot;
using System;

public partial class CharacterSelect : Control
{	
	private MainMenu _menu;
	
	public override void _Ready()
	{
		_menu = GetParent() as MainMenu;
		
		GetNode<Button>("HBoxContainer/FoxButton").Pressed += FoxButtonPressed;
		GetNode<Button>("HBoxContainer/FrogButton").Pressed += FrogButtonPressed;
		GetNode<Button>("HBoxContainer/RabbitButton").Pressed += RabbitButtonPressed;
		GetNode<Button>("HBoxContainer/RaccoonButton").Pressed += RaccoonButtonPressed;
		
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
	
	private void FoxButtonPressed()
	{
		_menu.StartGame("fox");
	}
	private void FrogButtonPressed()
	{
		_menu.StartGame("frog");
	}
	private void RabbitButtonPressed()
	{
		_menu.StartGame("rabbit");
	}
	private void RaccoonButtonPressed()
	{
		_menu.StartGame("raccoon");
	}
	
}
