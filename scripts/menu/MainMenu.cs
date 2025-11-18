using Godot;
using System;

public partial class MainMenu : CanvasLayer
{	
	private Ui _ui;
	private CenterContainer _startScreen;
	private Control _characterSelectScreen;

	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		GetTree().Paused = true;

		_ui = GetNode<Ui>("../Ui");
		_ui.MainMenuRequestedUi += OnMainMenuRequested;
		
		_startScreen = GetNode<CenterContainer>("Start");
		_characterSelectScreen = GetNode<Control>("CharacterSelect");
		
		_characterSelectScreen.Visible = false;
		
		GetNode<Button>("Start/VBoxContainer/Button").Pressed += StartPressed;
	}

	
	// start the game
	public void StartGame(string character)
	{
		this.Visible = false;
		GetTree().Paused = false;
		GameManager.Instance.StartNewGame(character);
	}
	
	public void Reset()
	{
		_characterSelectScreen.Visible = false;
		_startScreen.Visible = true;
		this.Visible = true;
	}

	// go to character selection
	private void StartPressed()
	{
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
	
	private void OnMainMenuRequested()
	{
		_characterSelectScreen.Visible = false;
		_startScreen.Visible = true;
		this.Visible = true;
	}
}
