using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameJam25.scripts.world_terrain;

public partial class WorldTerrain : Node2D
{
    [Export] int[] TerrainTileTypes;
    [Export] float[] TerrainTileTypeChance;

    [Export] TileMapLayer BaseTileMapLayer;
    [Export] TileMapLayer ObjectTileMapLayer;

    [Export] PackedScene RacoonShrine;
    [Export] PackedScene RabbitShrine;
    [Export] PackedScene OwlShrine;

    [Export] int NumShrines;

    [Export] Curve pathCurve;

    [Export] int pathCurveSize;

    private PackedScene[] allShrinePckdScns = new PackedScene[3];

    private List<Vector2I> shrinesRowCol = new List<Vector2I>();

    private bool[,] islandVisitedCells;

    private bool[,] pathVisitedCells;

    private List<List<Vector2I>> islands = new List<List<Vector2I>>();


    [Export] int MapSizeRows;
    [Export] int MapSizeCols;

    [Export] int ShrineSizeRows;
    [Export] int ShrineSizeCols;
    [Export] int MinimumDistanceShrines; // in tile cells

    [Export] float tileSizeXPxl;
    [Export] float tileSizeYPxl;


    // atlas coords for versions of each tile
    private Vector2I[] baseLayerGrassTiles =
        { new Vector2I(4, 0), new Vector2I(9, 0), new Vector2I(12, 0), new Vector2I(10, 0) };

    private Vector2I[] baseLayerFlowerTiles = { new Vector2I(5, 0), new Vector2I(13, 0), new Vector2I(7, 0) };
    //private Vector2I[] baseLayerFlowerTiles = { new Vector2I(15, 0) };
    private Vector2I[] baseLayerDirtTiles = { new Vector2I(0, 0), new Vector2I(3, 0) };

    private Vector2I[] objectLayerGrassTiles =
        { new Vector2I(4, 1), new Vector2I(5, 1), new Vector2I(6, 1), new Vector2I(7, 1) };

    private Vector2I[] objectLayerDirtTiles = { };

    private enum WorldDataStates
    {
        Grass,
        Flowers,
        Dirt,
    };

    private Vector2I[][] baseLayerTilesMapToState = new Vector2I[3][];
    private Vector2I[][] objectLayerTilesMapToState = new Vector2I[3][];

    private int[,] worldData;
    
    private void markShrinesWorldData()
    {
        // pick 3 sub 2d arrays in mapData with minimum distance apart
        // CAREFUL too big min distance can create inf loop here.
        List<Vector2I> shrinePlacementMapDataCoord = new List<Vector2I>(); // X is COl Y is ROW
        while (shrinePlacementMapDataCoord.Count < 3)
        {
            Vector2I newCoord = new Vector2I(
                GD.RandRange(0, MapSizeCols - ShrineSizeCols),
                GD.RandRange(0, MapSizeRows - ShrineSizeRows));
            bool validCoord = true;
            foreach (Vector2I coord in shrinePlacementMapDataCoord)
            {
                if ((coord - newCoord).Length() < MinimumDistanceShrines)
                {
                    validCoord = false;
                    break;
                }
            }

            if (validCoord)
            {
                shrinePlacementMapDataCoord.Add(newCoord);
                int row = newCoord.Y;
                int col = newCoord.X;
                for (int i = row; i < row + ShrineSizeRows; i++)
                {
                    for (int j = col; j < col + ShrineSizeCols; j++)
                    {
                        worldData[i, j] = -1;
                    }
                }

                shrinesRowCol.Add(new Vector2I(row, col));
            }
        }
    }

    private void printWorldData()
    {
        for (int i = 0; i < worldData.GetLength(0); i++)
        {
            string curr_row = "";
            for (int j = 0; j < worldData.GetLength(1); j++)
            {
                curr_row += worldData[i, j];
            }

            GD.Print(curr_row);
        }
    }

    private void populateBaseLayer()
    {
        for (int row = 0; row < worldData.GetLength(0); row++)
        {
            for (int col = 0; col < worldData.GetLength(1); col++)
            {
                int worldDataState = worldData[row, col];
                if (worldDataState == -1) continue;
                Vector2I[] tileOptions = baseLayerTilesMapToState[worldDataState];
                BaseTileMapLayer.SetCell(new Vector2I(row, col), 0, tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void populateObjectLayer()
    {
        for (int row = 0; row < worldData.GetLength(0); row++)
        {
            for (int col = 0; col < worldData.GetLength(1); col++)
            {
                int worldDataState = worldData[row, col];
                if (worldDataState == -1) continue;
                Vector2I[] tileOptions = objectLayerTilesMapToState[worldDataState];
                if ((WorldDataStates)worldDataState == WorldDataStates.Grass && GD.Randf() < .2)
                {
                    ObjectTileMapLayer.SetCell(new Vector2I(row, col), 0, tileOptions[GD.Randi() % tileOptions.Length]);
                }
            }
        }
    }

    private void spawnShrines()
    {
        for (int i = 0; i < allShrinePckdScns.Length; i++)
        {
            Node2D currShrine = allShrinePckdScns[i].Instantiate<Node2D>();
            Vector2I centerTile = new Vector2I(
                shrinesRowCol[i].X + (ShrineSizeCols / 2),
                shrinesRowCol[i].Y + (ShrineSizeRows / 2)
            );
            currShrine.Position = BaseTileMapLayer.MapToLocal(centerTile);
            AddChild(currShrine);
        }
    }

    private void populateMap()
    {
        populateBaseLayer();
        populateObjectLayer();
        spawnShrines();
    }

    private void wipeMap()
    {
        for (int i = 0; i < worldData.GetLength(0); i++)
        {
            for (int j = 0; j < worldData.GetLength(1); j++)
            {
                ObjectTileMapLayer.EraseCell(new Vector2I(i, j));
                BaseTileMapLayer.EraseCell(new Vector2I(i, j));
            }
        }
    }

    public override void _Ready()
    {
        worldData = new int[MapSizeCols, MapSizeRows]; // map size
        islandVisitedCells = new bool[MapSizeCols, MapSizeRows];
        pathVisitedCells = new bool[MapSizeCols, MapSizeRows];

        allShrinePckdScns[0] = RacoonShrine;
        allShrinePckdScns[1] = RabbitShrine;
        allShrinePckdScns[2] = OwlShrine;

        baseLayerTilesMapToState[0] = baseLayerGrassTiles;
        baseLayerTilesMapToState[1] = baseLayerFlowerTiles;
        baseLayerTilesMapToState[2] = baseLayerDirtTiles;

        objectLayerTilesMapToState[0] = objectLayerGrassTiles;
        objectLayerTilesMapToState[1] = objectLayerDirtTiles;

        Matrix matrix =
            ProcWorldGeneration.InitWorldData(MapSizeRows, MapSizeCols, TerrainTileTypes, TerrainTileTypeChance);
        matrix = ProcWorldGeneration.SmoothWorldData(matrix, TerrainTileTypes, 1, true);
        matrix = ProcWorldGeneration.SmoothWorldData(matrix, TerrainTileTypes, 1, true);
        matrix = ProcWorldGeneration.SmoothWorldData(matrix, TerrainTileTypes, 1, true);
        matrix = ProcWorldGeneration.SmoothWorldData(matrix, TerrainTileTypes, 1, true);

        IslandConnector.ConnectIslands(matrix, 0, 5, pathCurveSize, pathCurve);
        
        
        markShrinesWorldData();
        populateMap();

        
        //printWorldData();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("regen"))
        {
            wipeMap();
            
            GD.Print("Island Count");
            GD.Print(islands.Count);
            
            populateMap();
        }
    }
}