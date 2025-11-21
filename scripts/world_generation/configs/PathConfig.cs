using Godot;

namespace GameJam25.scripts.world_generation.configs
{
	public struct PathConfig(int pathRadius, int pathCurveSize, Curve pathCurve)
	{
		public int PathRadius { get; } = pathRadius;
		public int pathCurveSize { get; } = pathCurveSize;
		public Curve pathCurve { get; } = pathCurve;
	}
}
