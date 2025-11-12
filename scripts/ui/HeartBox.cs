using Godot;
using System;

public partial class HeartBox : HFlowContainer
{
	[Export] public Texture2D HeartTexture;
	[Export] public Vector2 HeartSize = new Vector2(24, 24); // target heart size
	[Export] public int MaxHearts = 5;
	[Export] public int HSpacing = 4; // horizontal spacing between hearts

	private int _currentHearts;

	public override void _Ready()
	{
		_currentHearts = MaxHearts;
		UpdateHearts();
	}

	public void SetHearts(int amount)
	{
		_currentHearts = Mathf.Clamp(amount, 0, MaxHearts);
		UpdateHearts();
	}

	private void UpdateHearts()
	{
		// Clear existing hearts
		//foreach (Node child in GetChildren())
			//child.QueueFree();

		// Add hearts at fixed size; HFlowContainer will wrap them automatically
		for (int i = 0; i < _currentHearts; i++)
		{
			var heart = new TextureRect
			{
				Texture = HeartTexture,
				CustomMinimumSize = HeartSize,                 // controls layout size
				Size = HeartSize,                              // set initial size immediately
				StretchMode = TextureRect.StretchModeEnum.Scale // scale texture to rect
			};

			// Prevent the child from expanding to fill lines
			//heart.SizeFlagsHorizontal = (int)Control.SizeFlags.None;
			//heart.SizeFlagsVertical   = (int)Control.SizeFlags.None;

			//AddChild(heart);
		}
	}
}
