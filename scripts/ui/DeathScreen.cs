using Godot;
using System;

public partial class DeathScreen : Panel
{
	[Signal]
	public delegate void MainMenuRequestedEventHandler();
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Visible = false;
		GetNode<Button>("CenterContainer/VBoxContainer/Menu").Pressed += EmitSignalMainMenuRequested;
	}
	
	public void Open() {
		Visible = true;
		Modulate = new Color(Modulate, 1); // ensure alpha is 1 (visible)
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1f, 1.0).From(0f);
	}

	public void Close()
	{
		Visible = false;
	}
	
}
