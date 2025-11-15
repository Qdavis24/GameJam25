using Godot;

public partial class Camera : Camera2D
{
    [Export] private float _speed;
    [Export] private float _shakeAmount = 10f;
    [Export] private float _shakeDuration = 0.3f;

    public Node2D Target;
    
    private float _shakeTimer = 0f;
    private Vector2 _originalOffset;

    public override void _Ready()
    {
        PositionSmoothingEnabled = false;
        _originalOffset = Offset;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Target != null)
        {
            GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition, _speed);
        }

        // Handle shake
        if (_shakeTimer > 0)
        {
            _shakeTimer -= (float)delta;
            Offset = _originalOffset + new Vector2(
                (GD.Randf() * 2 - 1) * _shakeAmount,
                (GD.Randf() * 2 - 1) * _shakeAmount
            );
        }
        else
        {
            Offset = _originalOffset;
        }
    }

    public void Shake()
    {
        _shakeTimer = _shakeDuration;
    }
    
    public void Shake(float amount)
    {
        _shakeAmount = amount;
        _shakeDuration = .3f;
        _shakeTimer = .3f;
    }
}