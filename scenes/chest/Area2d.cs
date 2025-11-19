using Godot;
using System;

public partial class Area2d : Area2D
{
	private bool _opened = false;

	public override void _Ready()
	{
		// Optional: connect via code, but signals in inspector also work
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_opened)
			return;

		// Only react to player
		if (!body.IsInGroup("Players"))
			return;

		_opened = true;
		OpenChest();
	}

	private void OpenChest()
	{
		GameManager.Instance.OpenChest();
		this.QueueFree();
	}
}
