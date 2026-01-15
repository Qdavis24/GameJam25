using Godot;
using System;
using GameJam25.scripts;

public abstract partial class Pickup : Node2D
{
	[Export] private Timer _lifetime;
	[Export] private int _maxValue = 10;
	[Export] private ColorRect _colorRect;
	[Export] private GpuParticles2D _particles;
	
	public abstract PickupType Type { get; }
	public int Amount;
	public bool InPool = true;
	protected Player _player;

	public override void _Ready()
	{
		_lifetime.Timeout += () => GameManager.Instance.PickupPool.ReturnPickup(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (InPool) return;
		var dir = (_player.GlobalPosition - GlobalPosition);
		var distanceFromPlayer = dir.Length();
		if (distanceFromPlayer <= _player.PickupRange)
		{
			RewardPlayer();
			GameManager.Instance.PickupPool.ReturnPickup(this);
		}
		else if (dir.Length() <= _player.PickupAttractRange)
		{
			dir = dir.Normalized();
			GlobalPosition += dir * 300.0f * (float) delta;
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
	}

	protected abstract void RewardPlayer();
}
