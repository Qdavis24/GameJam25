using Godot;
using System;
using GameJam25.scripts.singletons;

public partial class ObstacleLayer : TileMapLayer
{
    public override void _Process(double delta)
    {

        if (Material is ShaderMaterial shader)
        {
            shader.SetShaderParameter("player_position", PlayerData.Position);
        }
            
    }
}