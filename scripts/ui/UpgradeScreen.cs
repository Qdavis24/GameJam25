using Godot;
using System;

public partial class UpgradeScreen : HBoxContainer
{
	// BUTTONS
	private UpgradeButton _button1;
	private UpgradeButton _button2;
	private UpgradeButton _button3;

	public void Show()
	{
		this.Visible = true;
		_button1.SpinAsync();
		_button2.SpinAsync();
		_button3.SpinAsync();
	}

	public override void _Ready()
	{
		this.Visible = false;
		_button1 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton");
		_button2 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton2");
		_button3 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton3");
	}

	public override void _Process(double delta)
	{
	}
}
