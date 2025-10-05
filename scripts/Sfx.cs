using Godot;

public partial class Sfx : Node
{
	public static Sfx I; // optional static access

	public override void _EnterTree() => I = this;

	private bool _footstepPlaying = false;

	public void PlayFootstep(AudioStream stream, Vector2 pos)
	{
		// Skip if the previous step hasn't finished
		if (_footstepPlaying || stream == null)
			return;

		var p = new AudioStreamPlayer2D
		{
			Stream = stream,
			GlobalPosition = pos,
			VolumeDb = -6f,
			PitchScale = (float)GD.RandRange(0.9, 1.1)
		};

		AddChild(p);
		_footstepPlaying = true;

		// Reset flag when finished
		p.Finished += () =>
		{
			_footstepPlaying = false;
			p.QueueFree();
		};

		p.Play();
	}
	
	public void Play2D(AudioStream stream, Vector2 pos, float volumeDb = -6f, float pitch = 1f)
	{
		if (stream == null) return;

		var p = new AudioStreamPlayer2D
		{
			Stream = stream,
			GlobalPosition = pos,
			VolumeDb = volumeDb,
			PitchScale = pitch
		};

		AddChild(p);              // lives under the autoload (not your attack node)
		p.Finished += () => p.QueueFree();
		p.Play();
	}
}
