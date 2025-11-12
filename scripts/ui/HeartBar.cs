using Godot;
using System;

public partial class HeartBar : HBoxContainer
{
	[Export] public Texture2D HeartTexture;
	[Export] public Vector2 HeartSize = new Vector2(24, 24); // adjust in Inspector

	private int MaxHearts = 5;
	private int currentHearts;

	public override void _Ready()
	{
		currentHearts = MaxHearts;
		UpdateHearts();
	}

	public void SetHearts(int amount)
	{
		currentHearts = Mathf.Clamp(amount, 0, MaxHearts);
		UpdateHearts();
	}

	private void UpdateHearts()
	{
		// Clear existing hearts
		foreach (Node child in GetChildren())
			//child.QueueFree();

		// Add hearts
		for (int i = 0; i < currentHearts; i++)
		{
			var heart = new TextureRect
			{
				Texture = HeartTexture,
				// Make the control's rect this exact size
				CustomMinimumSize = HeartSize,
				StretchMode = TextureRect.StretchModeEnum.Scale, // scale texture to rect size
				// don't use Expand/ExpandMode; we want a fixed-size rect
			};

			// Also set the actual size so it starts at HeartSize immediately
			heart.Size = HeartSize;

			//AddChild(heart);
		}
	}
}
