using Godot;
using System;

public partial class DeathScreen : Panel
{
	[Signal]
	public delegate void MainMenuRequestedEventHandler();

	[Export] private AudioStream _deathSound;
	
	private Label _killCount;
	private Label _levelCount;
	
	private bool _mainMenuBeenPressed = false;

	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Visible = false;
		
		_killCount = GetNode<Label>("CenterContainer/VBoxContainer/Stats/KillCount");
		_levelCount = GetNode<Label>("CenterContainer/VBoxContainer/Stats/LevelCount");
		
		GetNode<Button>("CenterContainer/VBoxContainer/Menu").Pressed += MainMenuPressed;
	}


	public void Open((string kills, string level) counters)
	{
		_mainMenuBeenPressed = false;

		_killCount.Text = counters.kills;
		_levelCount.Text = counters.level;
		Visible = true;
		Sfx.I.PlayUi(_deathSound);
		Modulate = new Color(Modulate, 1); // ensure alpha is 1 (visible)
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1f, 1.0).From(0f);
	}

	public void Close()
	{
		Visible = false;

	}
	
	public void MainMenuPressed()
	{
		if (_mainMenuBeenPressed) return;
		_mainMenuBeenPressed = true;
		EmitSignalMainMenuRequested();
	}
}
