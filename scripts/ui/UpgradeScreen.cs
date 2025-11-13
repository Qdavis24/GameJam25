using Godot;
using System;

public partial class UpgradeScreen : HBoxContainer
{
	// BUTTONS
	private UpgradeButton _button1;
	private UpgradeButton _button2;
	private UpgradeButton _button3;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_button1 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton");
		_button2 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton2");
		_button3 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton3");

		_button1.SpinAsync();
		_button2.SpinAsync();
		_button3.SpinAsync();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
