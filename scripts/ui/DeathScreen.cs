using Godot;
using System;

public partial class DeathScreen : Panel
{
	[Signal]
	public delegate void MainMenuRequestedEventHandler();
	
	[Export] private AudioStream _deathSound;
	
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		this.Visible = false;
		GetNode<Button>("CenterContainer/VBoxContainer/Menu").Pressed += MenuPressed;
	}
	
	public void Show() {
		GetTree().Paused = true;
		this.Visible = true;
		Sfx.I.PlayUi(_deathSound);
		Modulate = new Color(Modulate, 1); // ensure alpha is 1 (visible)
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1f, 1.0).From(0f);
	}

	private void MenuPressed()
	{
		EmitSignal(SignalName.MainMenuRequested);
	}
}
