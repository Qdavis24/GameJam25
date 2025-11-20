using Godot;
using System;
using System.Collections.Generic;

public partial class ChatBubble : Panel
{
	private Timer _blinkTimer;
	private Label _label;

	// List of messages the bubble can show
	private List<string> _messages = new()
	{
		"Help, he locked me up!",
		"Get the key from him!",
		"Take him out!",
	};
	
	private int _messageIndex = 0;

	// Visible & hidden durations
	private const float VisibleTime = 2f;
	private const float HiddenTime  = 3f;

	public override void _Ready()
	{	
		_label = GetNode<Label>("CenterContainer/Label");
		
		_messageIndex = _messages.Count - 1;

		_blinkTimer = new Timer { OneShot = false };
		_blinkTimer.Timeout += OnBlinkTimeout;
		AddChild(_blinkTimer);

		// Start visible with an initial message
		Visible = true;
		_label.Text = GetNextMessage();

		_blinkTimer.WaitTime = VisibleTime;
		_blinkTimer.Start();
	}

	private void OnBlinkTimeout()
	{
		if (GameManager.Instance.NumSpawners < 3)
		{
			QueueFree();
		}
		
		// Flip visibility
		Visible = !Visible;

		if (Visible)
		{
			// We just became visible → change message
			_label.Text = GetNextMessage();
		}

		// Adjust timer length
		_blinkTimer.WaitTime = Visible ? VisibleTime : HiddenTime;
	}

	private string GetNextMessage()
	{
		if (_messageIndex == (_messages.Count - 1))
		{
			_messageIndex = 0;
		} 
		else {
			_messageIndex += 1;
		}
		
		return _messages[_messageIndex];
	}
}
