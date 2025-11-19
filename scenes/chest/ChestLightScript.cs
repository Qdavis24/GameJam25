using Godot;

public partial class ChestShimmerLight : PointLight2D
{
	[Export] public float BaseEnergy = 3.0f;     // starting brightness
	[Export] public float PulseStrength = 0.4f;  // how big the shimmer swings
	[Export] public float PulseSpeed = 2.0f;     // speed of shimmer

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
