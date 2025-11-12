using Godot;
using System;

public partial class RenderOptions : VBoxContainer
{
	private UpgradeButton _button1;
	private UpgradeButton _button2;
	private UpgradeButton _button3;

	public override void _Ready()
	{
		_button1 = GetNode<UpgradeButton>("UpgradeButton");
		_button2 = GetNode<UpgradeButton>("UpgradeButton2");
		_button3 = GetNode<UpgradeButton>("UpgradeButton3");

		// _button1.SpinAsync();
		// _button2.SpinAsync();
		// _button3.SpinAsync();
	}
}
