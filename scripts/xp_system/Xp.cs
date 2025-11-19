using Godot;
using System;

public partial class Xp : Area2D
{
	[Export] private int _maxValue = 10;
	[Export] private ColorRect _colorRect;
	[Export] private GpuParticles2D _particles;

	public int Amount;

	public override void _Ready()
	{
		_colorRect.Visible = false;
		Monitorable = false;
		Monitoring = false;
	}
	public void Disable()
	{
		_colorRect.Visible = false;
		_particles.Emitting = false;  // Stops GPU from emitting new particles
		_particles.ProcessMode = Node.ProcessModeEnum.Disabled;  // Stops all processing
		Monitorable = false;
	}

	public void Enable()
	{
		_colorRect.Visible = true;
		_particles.Emitting = true;  // GPU starts emitting again
		_particles.ProcessMode = Node.ProcessModeEnum.Inherit;
		Amount = GD.RandRange((int)(_maxValue*.5f), _maxValue);
		Monitorable = true;
	}
}
