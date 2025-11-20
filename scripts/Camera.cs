using System;
using Godot;

public partial class Camera : Camera2D
{
	[Export] private float _speed;
	[Export] private float _shakeAmount = 10f;

	[Export] private float _deathAnimDuration = 10f;
	[Export] private float _deathAnimRotation = -Mathf.Pi/7;
	[Export] private float _deathZoom = .3f;

	public Node2D Target;
	
	private float _shakeTimer;
	private Vector2 _originalOffset;

	private float _deathTimer;
	private Vector2 _originalZoom;
	private float _originalRotation;

	public override void _Ready()
	{
		PositionSmoothingEnabled = false;
		_originalOffset = Offset;
		_originalZoom = Zoom;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Target != null)
		{
			GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition, _speed);
		}

		// Handle shake
		if (_shakeTimer > 0)
		{
			_shakeTimer -= (float)delta;
			Offset = _originalOffset + new Vector2(
				(GD.Randf() * 2 - 1) * _shakeAmount,
				(GD.Randf() * 2 - 1) * _shakeAmount
			);
		}
		else
		{
			Offset = _originalOffset;
		}
		
		// Handle death anim
		if (_deathTimer > 0)
		{
			_deathTimer -= (float)delta;
			var progress = _deathTimer / _deathAnimDuration;
			Zoom *= 1 + progress * _deathZoom * (float)delta;
			Rotation += progress * _deathAnimRotation * (float)delta;
		}
	}
	
	public void Shake(float amount, float duration=.3f)
	{
		_shakeAmount = amount;
		_shakeTimer = duration;
	}

	public void DeathAnim()
	{
		_deathTimer = _deathAnimDuration;
	}
	
}
