using Godot;
using System;
using System.Collections.Generic;
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

    private PackedScene[] allShrinePckdScns = new PackedScene[3];

    private List<Vector2I> shrinesRowCol = new List<Vector2I>();

    [Export] int MapSizeRows;
    [Export] int MapSizeCols;

    [Export] int ShrineSizeRows;
    [Export] int ShrineSizeCols;
    [Export] int MinimumDistanceShrines; // in tile cells

    [Export] float tileSizeXPxl;
    [Export] float tileSizeYPxl;


    // atlas coords for versions of each tile
    private Vector2I[] baseLayerGrassTiles = { new Vector2I(4, 0), new Vector2I(9, 0), new Vector2I(12, 0), new Vector2I(10, 0) };
    private Vector2I[] baseLayerFlowerTiles = { new Vector2I(5, 0), new Vector2I(13, 0), new Vector2I(7, 0) };
    private Vector2I[] baseLayerDirtTiles = { new Vector2I(0, 0), new Vector2I(3, 0) };
    private Vector2I[] objectLayerGrassTiles = { new Vector2I(4, 1), new Vector2I(5, 1), new Vector2I(6, 1), new Vector2I(7, 1) };
    private Vector2I[] objectLayerDirtTiles = { };

    private enum WorldDataStates
    {
        Grass,
        Flowers,
        Dirt,
    };

    private Vector2I[][] baseLayerTilesMapToState = new Vector2I[3][];
    private Vector2I[][] objectLayerTilesMapToState = new Vector2I[3][];

    private int[,] worldData; // array to represent how to build our map
    private bool uniformNeighbors(int sample_row, int sample_col, int neighbor_depth)
    {
        for (int row_shift = -neighbor_depth; row_shift <= neighbor_depth; row_shift++)
        {
            for (int col_shift = -neighbor_depth; col_shift <= neighbor_depth; col_shift++)
            {
                int curr_row = sample_row + row_shift;
                int curr_col = sample_col + col_shift;
                if (curr_row == sample_row && curr_col == sample_col) continue;
                if (curr_row < 0) curr_row = worldData.GetLength(0) + curr_row;
                if (curr_row >= worldData.GetLength(0)) curr_row -= worldData.GetLength(0);
                if (curr_col < 0) curr_col = worldData.GetLength(1) + curr_col;
                if (curr_col >= worldData.GetLength(1)) curr_col -= worldData.GetLength(1);
                if (worldData[curr_row, curr_col] != worldData[sample_row, sample_col]) return false;
            }
        }
        return true;

    }
    private void initWorldData()
    {
        for (int i = 0; i < worldData.GetLength(0); i++)
        {
            for (int j = 0; j < worldData.GetLength(1); j++)
            {
                float rInt = GD.Randf();
                float cummulative = 0f;
                for (int k = 0; k < TerrainTileTypes.Length; k++)
                {
                    cummulative += TerrainTileTypeChance[k];
                    if (rInt <= cummulative)
                    {
                        worldData[i, j] = TerrainTileTypes[k];
                        break;
                    }
                }
            }
        }
    }

    private void smoothWorldData(int changeWeight)
    {
        int[,] copyTerrain = (int[,])worldData.Clone();
        for (int i = 0; i < worldData.GetLength(0); i++)
        {
            for (int j = 0; j < worldData.GetLength(1); j++)
            {
                int[] counts = new int[TerrainTileTypes.Length];
                for (int row_shift = -1; row_shift <= 1; row_shift++)
                {
                    for (int col_shift = -1; col_shift <= 1; col_shift++)
                    {
                        int row = i + row_shift;
                        int col = j + col_shift;
                        if (row == i && col == j) continue;
                        if (row < 0) row = worldData.GetLength(0) + row;
                        if (row >= worldData.GetLength(0)) row -= worldData.GetLength(0);
                        if (col < 0) col = worldData.GetLength(1) + col;
                        if (col >= worldData.GetLength(1)) col -= worldData.GetLength(1);
                        counts[worldData[row, col]] += 1;
                    }
                }
                int currMaxI = 0;
                int currMax = 0;
                for (int k = 0; k < counts.Length; k++)
                {
                    // if (counts[k] >= changeWeight)
                    // {
                    //     copyTerrain[i, j] = k;
                    //     break;
                    // }
                    if (counts[k] > currMax)
                    {
                        currMax = counts[k];
                        currMaxI = k;
                    }
                }
                copyTerrain[i, j] = currMaxI;

            }
        }
        worldData = copyTerrain;
    }


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
                if ((WorldDataStates)worldDataState == WorldDataStates.Grass && GD.Randf() < .5 && uniformNeighbors(row, col, 2))
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
    public override void _Ready()
    {
        worldData = new int[MapSizeCols, MapSizeRows]; // map size

        allShrinePckdScns[0] = RacoonShrine;
        allShrinePckdScns[1] = RabbitShrine;
        allShrinePckdScns[2] = OwlShrine;

        baseLayerTilesMapToState[0] = baseLayerGrassTiles;
        baseLayerTilesMapToState[1] = baseLayerFlowerTiles;
        baseLayerTilesMapToState[2] = baseLayerDirtTiles;

        objectLayerTilesMapToState[0] = objectLayerGrassTiles;
        objectLayerTilesMapToState[1] = objectLayerDirtTiles;


        initWorldData();
        smoothWorldData(5);
        smoothWorldData(5);
        smoothWorldData(6);
        smoothWorldData(6);
        smoothWorldData(7);
        markShrinesWorldData();

        //printWorldData();
        populateMap();


    }
}
