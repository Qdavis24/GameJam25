using Godot;
using System;
using System.Threading.Tasks;

public partial class CanvasModulate : Godot.CanvasModulate
{
    [Export] private Color _normalColor = new Color(0.6f, 0.6f, 0.7f);
    [Export] private float _fadeDuration = 0.5f;
    
    public override void _Ready()
    {
        Color = _normalColor;
    }
    
    public async Task LevelTransition()
    {
        // Fade to black
        var tween = CreateTween();
        tween.TweenProperty(this, "color", Colors.Black, _fadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        // Level change happens here (GameManager does this between fade out/in)
        
        // Fade back to normal
        tween = CreateTween();
        tween.TweenProperty(this, "color", _normalColor, _fadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}