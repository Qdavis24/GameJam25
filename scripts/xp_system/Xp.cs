using Godot;
using System;

public partial class Xp : Area2D
{
	[Export] private Timer _lifetime;
	[Export] private int _maxValue = 10;
	[Export] private ColorRect _colorRect;
	[Export] private GpuParticles2D _particles;

	public int Amount;
	public bool InPool = true;
	private Player _player;

	public override void _Ready()
	{
		_lifetime.Timeout += () => GameManager.Instance.XpPool.ReturnXp(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (InPool) return;
		var dir = (_player.GlobalPosition - GlobalPosition);
		var distanceFromPlayer = dir.Length();
		if (distanceFromPlayer <= _player.PickupRange)
		{
			_player.Xp += Amount;
			GameManager.Instance.XpPool.ReturnXp(this);
		}
		else if (dir.Length() <= _player.PickupAttractRange)
		{
			dir.Normalized();
			GlobalPosition += dir * 5.0f * (float) delta;
		}
	}

	// Pool methods
	public void Disable()
	{
		InPool = true;
		_lifetime.Stop();
		_colorRect.Visible = false;
		_particles.Emitting = false;  // Stops GPU from emitting new particles
		_particles.ProcessMode = Node.ProcessModeEnum.Disabled;  // Stops all processing
		Monitorable = false;
	}

	public void Enable()
	{
		InPool = false;
		_lifetime.Start();
		_player = GameManager.Instance.Player;
		_colorRect.Visible = true;
		_particles.Emitting = true;  // GPU starts emitting again
		_particles.ProcessMode = Node.ProcessModeEnum.Inherit;
		Amount = GD.RandRange((int)(_maxValue*.5f), _maxValue);
		Monitorable = true;
	}
}
