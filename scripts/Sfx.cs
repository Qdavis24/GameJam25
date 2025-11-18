using Godot;

public partial class Sfx : Node
{
	public static Sfx I; // optional static access

	public override void _EnterTree() => I = this;

	private bool _footstepPlaying = false;
	
	private const string SfxBus = "SFX";
	private const string MusicBus  = "Music";

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
			PitchScale = (float)GD.RandRange(0.9, 1.1),
			Bus = SfxBus
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
		if (stream == null)
		{
			GD.PrintErr("SFX stream is null");
			return;
		}

		var p = new AudioStreamPlayer2D
		{
			Stream = stream,
			GlobalPosition = pos,
			VolumeDb = volumeDb,
			PitchScale = pitch,
			Bus = SfxBus
		};
		
		AddChild(p);
		p.Finished += () => p.QueueFree();
		p.Play();
	}
	
	public void PlayFollowing(AudioStream stream, Node2D target, float volumeDb = -6f, float pitch = 1f)
	{
		if (stream == null || target == null) return;

		var p = new AudioStreamPlayer2D
		{
			Stream = stream,
			VolumeDb = volumeDb,
			PitchScale = pitch,
			Bus = SfxBus
		};
	
		// Parent to the target so it follows automatically
		target.AddChild(p);
		p.Finished += () => p.QueueFree();
		p.Play();
	}
	public void PlayUi(AudioStream stream, float volumeDb = -6f, float pitch = 1f)
	{
		if (stream == null) return;

		var p = new AudioStreamPlayer
		{
			Stream = stream,
			VolumeDb = volumeDb,
			PitchScale = pitch,
			Bus = SfxBus
		};

		AddChild(p);
		p.Finished += () => p.QueueFree();
		p.Play();
	}
	
	public void PlayMusic()
	{
		
	}
	
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
	}
}
