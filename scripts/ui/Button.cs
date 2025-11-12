using Godot;
using System;

public partial class Button : Godot.Button
{
	public override void _Ready() => UpdateFontSize();

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
			UpdateFontSize();
	}

	private void UpdateFontSize()
	{
		var font = GetThemeFont("font");
		if (font == null || string.IsNullOrEmpty(Text))
			return;

		// Available content area = size minus theme margins
		float w = Size.X - (GetThemeConstant("content_margin_left") + GetThemeConstant("content_margin_right"));
		float h = Size.Y - (GetThemeConstant("content_margin_top") + GetThemeConstant("content_margin_bottom"));
		if (w <= 0 || h <= 0)
			return;

		// Start at current theme font size; shrink until it fits
		int size = Mathf.Max(1, GetThemeFontSize("font_size"));
		while (size > 1)
		{
			Vector2 textSize = font.GetStringSize(Text, HorizontalAlignment.Left, size);
			if (textSize.X <= w && textSize.Y <= h)
				break;
			size--;
		}

		if (GetThemeFontSize("font_size") != size)
			AddThemeFontSizeOverride("font_size", size);
	}
}
