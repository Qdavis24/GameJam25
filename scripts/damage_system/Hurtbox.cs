using Godot;
using System;

namespace GameJam25.scripts.damage_system;
public partial class Hurtbox : Area2D
{
    [Export] private float _iframeTime = .2f;
    private Timer _timer;
    public bool IsActive = true;
    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        _timer = new Timer();
        AddChild(_timer);
        _timer.WaitTime = _iframeTime;
        _timer.Timeout += () => Monitoring = true;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Hitbox hitbox)
        {
            SetDeferred(Area2D.PropertyName.Monitoring, false);
            _timer.Start();
        }
    }

}
