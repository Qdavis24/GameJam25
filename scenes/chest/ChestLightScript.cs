using Godot;

public partial class ChestLightScript : PointLight2D
{
	[Export] public float BaseEnergy = 0.5f;
	[Export] public float PulseStrength = 0.15f;
	[Export] public float PulseSpeed = 1.2f;

	private float _timeOffset;

	public override void _Ready()
	{
		// Makes each chest shimmer differently (not in sync)
		_timeOffset = GD.Randf() * 100f;

		Energy = BaseEnergy;
	}

	public override void _Process(double delta)
	{
		float t = (float)Time.GetTicksMsec() / 1000f + _timeOffset;

		// Smooth shimmer using a sine wave
		float shimmer = Mathf.Sin(t * PulseSpeed) * PulseStrength;

		Energy = BaseEnergy + shimmer;
	}
}
