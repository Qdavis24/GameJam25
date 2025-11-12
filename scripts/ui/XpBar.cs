using Godot;
using System;

public partial class XpBar : ProgressBar
{
	public override void _Ready()
	{
		var bg = new StyleBoxFlat
		{
			BgColor = new Color(0, 0, 0, 0.6f)
		};
		AddThemeStyleboxOverride("background", bg);

		var fill = new StyleBoxFlat
		{
			BgColor = new Color("00caf3ff"),
			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomLeft = 0,
			CornerRadiusBottomRight = 0
		};
		AddThemeStyleboxOverride("fill", fill);
	}
}
