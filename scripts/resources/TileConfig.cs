using Godot;
using System.Linq;

public partial class TileConfig : Resource
{
    [Export] private Vector2[] baseLayerWalkableTilesAtlasCoords;
    [Export] private Vector2[] baseLayerNonWalkableTilesAtlasCoords;

    [Export] private Vector2[] objectLayerWalkableTilesAtlasCoords;
    [Export] private Vector2[] objectLayerNonWalkableTilesAtlasCoords;
    
    [Export] public Vector2I DebugIslandAtlasCoord;
    [Export] public Vector2I DebugPathAtlasCoord;
    [Export] public Vector2I DebugIslandBorderAtlasCoord;
    
    [Export] public float TileSizeXPxl;
    [Export] public float TileSizeYPxl;
    
    // Public getters that return Vector2I
    public Vector2I[] BaseLayerWalkableTilesAtlasCoords => 
        baseLayerWalkableTilesAtlasCoords.Select(v => new Vector2I((int)v.X, (int)v.Y)).ToArray();
    
    public Vector2I[] BaseLayerNonWalkableTilesAtlasCoords => 
        baseLayerNonWalkableTilesAtlasCoords.Select(v => new Vector2I((int)v.X, (int)v.Y)).ToArray();

    public Vector2I[] ObjectLayerWalkableTilesAtlasCoords => 
        objectLayerWalkableTilesAtlasCoords.Select(v => new Vector2I((int)v.X, (int)v.Y)).ToArray();
    
    public Vector2I[] ObjectLayerNonWalkableTilesAtlasCoords => 
        objectLayerNonWalkableTilesAtlasCoords.Select(v => new Vector2I((int)v.X, (int)v.Y)).ToArray();
}