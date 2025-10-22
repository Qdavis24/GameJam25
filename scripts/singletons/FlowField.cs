using Godot;
namespace GameJam25.scripts.singletons;

public partial class FlowField : Node
{
    public int[,] WorldMatrix;
    public TileMapLayer BaseTileMapLayer;
    private CharacterBody2D _player;
    private Vector2 _lastPlayerPos;
    private float _updateFieldMag = 1000f;

    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(double delta)
    {
        if ((_lastPlayerPos - _player.GlobalPosition).LengthSquared() > _updateFieldMag)
        {
            
        }
    }

    private void GenerateFlowField()
    {
        var playerMatrixCoords = GenTileCoordinates(_player.GlobalPosition);
        GD.Print($"players curr cell : Row - {playerMatrixCoords.X}, Col - {playerMatrixCoords.Y}");
    }

    private Vector2I GenTileCoordinates(Vector2 pos)
    {
        return BaseTileMapLayer.LocalToMap(BaseTileMapLayer.ToLocal(pos));

    }
}