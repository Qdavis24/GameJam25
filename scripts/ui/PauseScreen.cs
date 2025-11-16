using Godot;
using System;

public partial class PauseScreen : Panel
{
	private bool _paused = false;
	
	private VBoxContainer _pauseContainer;
	private VBoxContainer _settingsContainer;
		
	public void Toggle()
	{
		_paused = !_paused;
		GetTree().Paused = _paused;
		this.Visible = _paused;
	}
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_pauseContainer = GetNode<VBoxContainer>("CenterContainer/VBoxContainer/pause");
		_settingsContainer = GetNode<VBoxContainer>("CenterContainer/VBoxContainer/settings");
		_settingsContainer.Visible = false;
		
		GetNode<Button>("CenterContainer/VBoxContainer/pause/Settings").Pressed += SettingsPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/pause/Menu").Pressed += MainMenuPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/pause/Unpause").Pressed += UnpausePressed;

		GetNode<Button>("CenterContainer/VBoxContainer/settings/LeaveSettings").Pressed += LeaveSettingsPressed;

		ProcessMode = Node.ProcessModeEnum.Always;
		this.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("pause"))
		{
			Toggle();
		}
	}
		
	private void UnpausePressed()
	{
		Toggle();
	}
	
	private void SettingsPressed() 
	{
		_pauseContainer.Visible = false;
		_settingsContainer.Visible = true;
	}
	
	private void MainMenuPressed()
	{
		GetTree().Quit(); // TODO: open main menu instead of quit
	}
	
	//////////// SETTINGS
	
	private void LeaveSettingsPressed()
	{
		_settingsContainer.Visible = false;
		_pauseContainer.Visible = true;
	}
}
