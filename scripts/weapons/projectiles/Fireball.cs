using Godot;


public partial class Fireball : Node2D
{
	[Export] private PackedScene _explosionParticles;
	[Export] private Area2D Hitbox;
	private Vector2 _currPos;

	public override void _Ready()
	{
		Hitbox.AreaEntered += OnAreaEntered;
	}
	

	private void OnAreaEntered(Node2D area)
	{
		if (area.IsInGroup("EnemyHurtbox"))
		{
			_currPos = GlobalPosition;
			QueueFree();
		}
	}

	public override void _ExitTree()
	{
		var explodeParticles = _explosionParticles.Instantiate<GpuParticles2D>();
		explodeParticles.Emitting = true;
		explodeParticles.Finished += () => explodeParticles.QueueFree();
		GetTree().Root.AddChild(explodeParticles);
		explodeParticles.GlobalPosition = GlobalPosition;
	}
}
