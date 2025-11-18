using Godot;
using System;
using System.Threading.Tasks;

// New script: ScreenFade.cs
public partial class ScreenFade : CanvasLayer
{
    [Export] private ColorRect _overlay;
    [Export] private float _fadeDuration = 2f;
    
    public override void _Ready()
    {
        _overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        Layer = 1000; // Render on top of everything
        _overlay.Modulate = new Color(1, 1, 1, 0); // Start transparent
    }
    
    public async Task FadeToBlack()
    {
        var tween = CreateTween();
        tween.TweenProperty(_overlay, "modulate:a", 1f, _fadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
    
    public async Task FadeToNormal()
    {
        var tween = CreateTween();
        tween.TweenProperty(_overlay, "modulate:a", 0f, _fadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
