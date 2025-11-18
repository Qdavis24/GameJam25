using Godot;
using System;

public partial class CharacterSelect : Control
{	
	private MainMenu _menu;
	
	[Export] public AudioStream FoxSound;
	[Export] public AudioStream FrogSound;
	[Export] public AudioStream RabbitSound;
	[Export] public AudioStream RaccoonSound;
	
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
		Sfx.I.PlayUi(FoxSound, -20f);
		_menu.StartGame("fox");
	}
	private void FrogButtonPressed()
	{
		Sfx.I.PlayUi(FrogSound, -20f);
		_menu.StartGame("frog");
	}
	private void RabbitButtonPressed()
	{
		Sfx.I.PlayUi(RabbitSound, -20f);
		_menu.StartGame("rabbit");
	}
	private void RaccoonButtonPressed()
	{
		Sfx.I.PlayUi(RaccoonSound, -10f);
		_menu.StartGame("raccoon");
	}
	
}
