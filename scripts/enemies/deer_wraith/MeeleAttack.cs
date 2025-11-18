using Godot;
using System;

public partial class MeeleAttack : Node2D
{
    [Export] private AudioStreamRandomizer _audioStreams;
    [Export] private AnimatedSprite2D _animSprite;

    private double _currTime;

    public override void _Ready()
    {
        Sfx.I.Play2D(_audioStreams, GlobalPosition, 10, .7f);
        _animSprite.Play();
        _animSprite.AnimationFinished += QueueFree;
    }
    
}
