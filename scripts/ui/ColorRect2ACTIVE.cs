using Godot;
using System;

public partial class ColorRect2ACTIVE : ColorRect
{
	private bool _hovered;
	private float _time;

	private Color _base;
	private readonly Color _blink = new Color(1f, 0.3f, 0.3f, 1f); // target color while blinking

	public override void _Ready()
	{
		// Make sure we can receive mouse events
		MouseFilter = MouseFilterEnum.Stop;

		// Subscribe to C# events (Godot 4)
		MouseEntered += OnMouseEntered;
		MouseExited  += OnMouseExited;

		_base = Color; // remember original
	}

	private void OnMouseEntered() => _hovered = true;

	private void OnMouseExited()
	{
		_hovered = false;
		_time = 0f;
		Color = _base; // reset
		SelfModulate = Colors.White; // just in case you modulated elsewhere
	}

	public override void _Process(double delta)
	{
		if (!_hovered) return;

		_time += (float)delta;

		// Smooth blink between base and _blink
		float t = (Mathf.Sin(_time * 6f) + 1f) * 0.5f; // 6 ≈ ~1Hz; raise to blink faster
		Color = _base.Lerp(_blink, t);
	}
}
