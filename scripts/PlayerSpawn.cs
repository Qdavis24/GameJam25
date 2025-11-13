using Godot;
using System;

public partial class PlayerSpawn : Node2D
{
	[Signal] public delegate void PortalOpenEventHandler();
	
	[Export] private PackedScene _playerPackedScene;
	[Export] GpuParticles2D particles;
	[Export] private PointLight2D light;
	[Export] private float maxLightEnergy;
	private double _currTime;

	public override void _Ready()
	{
		light.Energy = 0;
		particles.Emitting = true;
		particles.Finished += QueueFree;
	}
	public override void _PhysicsProcess(double delta)
	{
		_currTime += delta;
		var progress = (float) (_currTime / (2 * particles.Lifetime));
		light.Energy = (float) (1 + -Math.Cos(progress * Math.Tau)) * maxLightEnergy;
		if (progress == .5f)
		{
			EmitSignalPortalOpen();
		}
	}
}
