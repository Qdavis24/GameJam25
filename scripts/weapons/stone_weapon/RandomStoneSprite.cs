using Godot;
using System;

public partial class RandomStoneSprite : Sprite2D
{
	// List of image paths to pick from
	[Export] public string[] ImagePaths;

	public override void _Ready()
	{
		if (ImagePaths.Length == 0)
		{
			GD.PushWarning("RandomSprite: No images assigned!");
			return;
		}

		// Random index
		int index = (int)(GD.Randi() % ImagePaths.Length);

		// Load and apply texture
		Texture2D tex = ResourceLoader.Load<Texture2D>(ImagePaths[index]);
		Texture = tex;
	}
}
