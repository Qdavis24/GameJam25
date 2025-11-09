using Godot;
using System;

public partial class ObstacleLayer : TileMapLayer
{
    public override void _Process(double delta)
    {
        
        if (Material is ShaderMaterial shader && GameManager.Instance.Player != null)
        {
            shader.SetShaderParameter("player_position", GameManager.Instance.Player.Position);
        }
            
    }
}