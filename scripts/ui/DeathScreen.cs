using Godot;
using System;

public partial class DeathScreen : Panel
{
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		this.Visible = false;
		GetNode<Button>("CenterContainer/VBoxContainer/Menu").Pressed += MenuPressed;
	}
	
	public void Show() {
		GetTree().Paused = true;
		this.Visible = true;
		Modulate = new Color(Modulate, 1); // ensure alpha is 1 (visible)
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1f, 1.0).From(0f);
	}

	private void MenuPressed()
	{
		GetTree().Quit(); // TODO: open main menu instead of quit
	}
}
