using Godot;
using System;

public partial class Area2d : Area2D
{
	[Export] public Texture2D[] Pictures;
	
	private Sprite2D _sprite;

	public override void _Ready()
	{	
		BodyEntered += OnBodyEntered;
		
		_sprite = GetNode<Sprite2D>("Sprite2D");

		// Pick random picture
		if (Pictures != null && Pictures.Length > 0)
		{
			var tex = Pictures[GD.Randi() % Pictures.Length];
			_sprite.Texture = tex;
		}

		bool flip = GD.Randi() % 2 == 0;
		_sprite.FlipH = flip;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Players")) OpenChest();
	}

	private void OpenChest()
	{
		GameManager.Instance.OpenChest();
		this.QueueFree();
	}
}
