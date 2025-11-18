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
	
	private AudioStreamPlayer2D _audioStream;

	public override void _Ready()
	{	
		_audioStream = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		FadeInSound();
		light.Energy = 0;
		particles.Emitting = true;
		particles.Finished += Finished;
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
	
	private void Finished()
	{
		FadeOutSoundAndFree();
	}
	
	public async void FadeInSound(float duration = 1.5f, float targetVolume = -10f)
	{
		_audioStream.VolumeDb = -40f;       // start very quiet
		_audioStream.Play();
		
		var tween = CreateTween();
		tween.TweenProperty(_audioStream, "volume_db", targetVolume, duration);
		
		await ToSignal(tween, "finished");
	}
	
	public async void FadeOutSoundAndFree(float duration = 1.5f)
	{
		var tween = CreateTween();
		tween.TweenProperty(_audioStream, "volume_db", -40f, duration);

		await ToSignal(tween, "finished");
		_audioStream.Stop();
		
		QueueFree();
	}
}
