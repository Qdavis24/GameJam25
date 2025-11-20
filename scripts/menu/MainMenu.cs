using Godot;
using System;

public partial class MainMenu : CanvasLayer
{	
	[Signal] public delegate void InitGameEventHandler(string character);

	[Export] private AudioStream _clickSound;
	
	private CenterContainer _startScreen;
	private Control _characterSelectScreen;

	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		
		_startScreen = GetNode<CenterContainer>("Start");
		_characterSelectScreen = GetNode<Control>("CharacterSelect");
		
		_characterSelectScreen.Visible = false;
		
		GetNode<Button>("Start/VBoxContainer/Button").Pressed += StartPressed;
	}
	
	public void StartGame(string character) // start the game, should be called only by character select
	{
		EmitSignalInitGame(character);
	}
	
	public void Open()
	{
		_characterSelectScreen.Visible = false;
		_startScreen.Visible = true;
		this.Visible = true;
	}

	public void Close()
	{
		_characterSelectScreen.Visible = false;
		_startScreen.Visible = false;
		this.Visible = false;
	}

	// go to character selection
	private void StartPressed()
	{
		Sfx.I.PlayUi(_clickSound, -2);
		_startScreen.Visible = false;
		_characterSelectScreen.Visible = true;

		// Start fully transparent
		var color = _characterSelectScreen.Modulate;
		color.A = 0f;
		_characterSelectScreen.Modulate = color;

		// Tween its alpha from 0 → 1
		var tween = CreateTween();
		tween.TweenProperty(_characterSelectScreen, "modulate:a", 1f, 1.0f);
	}
	
}
