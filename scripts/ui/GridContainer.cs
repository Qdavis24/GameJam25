using Godot;
using System;

public partial class GridContainer : Godot.GridContainer
{
	[Export] public Texture2D HeartTexture;
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
		foreach (Node child in GetChildren())
		{
			if (child.Name == "SET_HEIGHT")
				continue; // skip it
				
			child.QueueFree();
		}
			

		for (int i = 0; i < _currentHearts; i++)
		{
			var heart = new TextureRect
			{
				Texture = HeartTexture,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
			};

			AddChild(heart);
		}
	}
}
