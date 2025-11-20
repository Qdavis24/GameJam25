using Godot;
using System;

public partial class ObstacleLayer : TileMapLayer
{
    [Export] private float _transparencyRadius = 150.0f;
    [Export] private float _fadeWidth = 50.0f;
    
    private Player _player;
    private ShaderMaterial _shaderMaterial;
    
    public override void _Ready()
    {
        _player = GameManager.Instance.Player;
        
        _shaderMaterial = Material as ShaderMaterial;
        
        if (_shaderMaterial == null)
        {
            GD.PrintErr("ObstacleLayer: No ShaderMaterial assigned!");
            return;
        }
        
        _shaderMaterial.SetShaderParameter("transparency_radius", _transparencyRadius);
        _shaderMaterial.SetShaderParameter("fade_width", _fadeWidth);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null || _shaderMaterial == null) return;
        
        // Just pass the global position directly - shader handles world space now
        _shaderMaterial.SetShaderParameter("player_position", _player.GlobalPosition);
    }
}