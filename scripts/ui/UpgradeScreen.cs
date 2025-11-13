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
	
	public void Hide()
	{
		this.Visible = false;
	}

	public override void _Ready()
	{
		this.Visible = false;
		
		_button1 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton");
		_button2 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton2");
		_button3 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton3");
		
		_button1.UpgradeClicked += Upgrade;
		_button2.UpgradeClicked += Upgrade;
		_button3.UpgradeClicked += Upgrade;
	}
	
	private void Upgrade(WeaponUpgrade weapon)
	{
		var ui = GetParent<Ui>();
		ui?.Upgrade(weapon);
		Hide();
	}

	public override void _Process(double delta)
	{
	}
}
