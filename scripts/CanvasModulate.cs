using System.Threading.Tasks;
using Godot;

public partial class CanvasModulate : Godot.CanvasModulate
{
    [Export] private Color _normalColor = new Color(0.6f, 0.6f, 0.7f);
    [Export] private float _fadeDuration = 2f;
    
    public override void _Ready()
    {
        Color = _normalColor;
    }
    
    public async Task FadeToBlack()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "color", Colors.Black, _fadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
    
    public async Task FadeToNormal()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "color", _normalColor, _fadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}