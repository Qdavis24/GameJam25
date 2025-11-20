using Godot;
using System;


public partial class PauseScreen : Panel
{
	[Signal] 
	public delegate void MainMenuRequestedEventHandler();

	[Signal] 
	public delegate void UnpauseEventHandler();
	
	private VBoxContainer _pauseContainer;
	private VBoxContainer _settingsContainer;
	
	private bool _mainMenuBeenPressed = false;

	public void Open()
	{
		_mainMenuBeenPressed = false;
		Visible = true;
	}

	public void Close()
	{
		Visible = false;
	}
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_pauseContainer = GetNode<VBoxContainer>("CenterContainer/VBoxContainer/pause");
		_settingsContainer = GetNode<VBoxContainer>("CenterContainer/VBoxContainer/settings");
		_settingsContainer.Visible = false;
		
		GetNode<Button>("CenterContainer/VBoxContainer/pause/Settings").Pressed += SettingsPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/pause/Menu").Pressed += MainMenuPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/pause/Unpause").Pressed += EmitSignalUnpause;

		string settings = "CenterContainer/VBoxContainer/settings/";
		GetNode<Button>(settings + "LeaveSettings").Pressed += LeaveSettingsPressed;
		
		double sfxCurrent = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("SFX"));
		GetNode<HSlider>(settings + "volume/HBoxContainer/SfxSlider").Value = sfxCurrent;
		
		double musicCurrent = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
		GetNode<HSlider>(settings + "music/HBoxContainer/MusicSlider").Value = musicCurrent;

		ProcessMode = Node.ProcessModeEnum.Always;
		Visible = false;
	}
	
	private void SettingsPressed() 
	{
		_pauseContainer.Visible = false;
		_settingsContainer.Visible = true;
	}
	
	private void MainMenuPressed()
	{
		if (_mainMenuBeenPressed) return;
		_mainMenuBeenPressed = true;
		EmitSignalMainMenuRequested();
	}
	
	//////////// SETTINGS
	
	private void LeaveSettingsPressed()
	{
		_settingsContainer.Visible = false;
		_pauseContainer.Visible = true;
	}
	
	
	private void _on_sfx_slider_value_changed(double value)
	{
		int bus = AudioServer.GetBusIndex("SFX");
		AudioServer.SetBusVolumeDb(bus, (float)value);
	}
	
	private void _on_music_slider_value_changed(double value)
	{
		int bus = AudioServer.GetBusIndex("Music");
		AudioServer.SetBusVolumeDb(bus, (float)value);
	}
}
