using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

    private void setNeighbors(int row, int col, int state, int numNeighbors)
    {
        for (int i = row - numNeighbors; i < row + numNeighbors; i++)
        {
            for (int j = col - numNeighbors; j < col + numNeighbors; j++)
            {
                if (i > 0 && i < worldData.GetLength(0) - 1 && j > 0 && j < worldData.GetLength(1) - 1)
                    worldData[i, j] = state;
            }
        }
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

    private void dfsRecordIsland(int row, int col)
    {
        if (row < 0 || row >= worldData.GetLength(0) || col < 0 || col >= worldData.GetLength(1) ||
            islandVisitedCells[row, col]) return;
        islandVisitedCells[row, col] = true;
        if (worldData[row, col] == (int)WorldDataStates.Flowers) islands[islands.Count - 1].Add(new Vector2I(col, row));
        else return;
        dfsRecordIsland(row, col - 1);
        dfsRecordIsland(row, col + 1);
        dfsRecordIsland(row - 1, col);
        dfsRecordIsland(row + 1, col);
    }

    private void findAllIslands()
    {
        for (int i = 0; i < worldData.GetLength(0); i++)
        {
            for (int j = 0; j < worldData.GetLength(1); j++)
            {
                if (worldData[i, j] == (int)WorldDataStates.Flowers && islandVisitedCells[i, j] != true)
                {
                    List<Vector2I> currIsland = new List<Vector2I>();
                    islands.Add(currIsland);
                    dfsRecordIsland(i, j);
                }
            }
        }
    }


    private void drawPathBetweenIslands(Vector2I island1ColRow, Vector2I island2ColRow, int pathRadius)
    {
        Vector2 start = new Vector2(island2ColRow.X, island2ColRow.Y);
        Vector2 end = new Vector2(island1ColRow.X, island1ColRow.Y);
        Vector2 direction = end - start;

        // Calculate total steps needed
        int totalSteps = (int)Math.Max(Math.Abs(direction.X), Math.Abs(direction.Y));
        if (totalSteps == 0) return; // Same position

        // Get perpendicular direction for curve offset
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X).Normalized();

        int islandState = worldData[island2ColRow.Y, island2ColRow.X];

        for (int step = 0; step <= totalSteps; step++)
        {
            float progress = (float)step / totalSteps; // 0 to 1

            // Get straight line position
            Vector2 straightPos = start + direction * progress;

            // Sample curve for bend amount
            float bendAmount = pathCurve.Sample(progress);

            // Apply perpendicular offset
            Vector2 curvedPos = straightPos + perpendicular * bendAmount * pathCurveSize;

            // Convert to grid coordinates
            int row = (int)Math.Round(curvedPos.Y);
            int col = (int)Math.Round(curvedPos.X);

            // Place the path tile
            setNeighbors(row, col, islandState, pathRadius);
        }
    }

    private Vector2I[] getTwoClosestCells(List<Vector2I> group1, List<Vector2I> group2)
    {
        /*
         * Returns the closest two cells in two given cell groupings based on the magnitude of their difference
         *
         * Input :
         * List<Vector2I> group1
         * A List containing Vector2I representing cells on a grid
         * List<Vector2I> group2
         * A List containing Vector2I representing cells on a grid
         *
         * Return :
         * Vector2I[2] closestCells
         * a Vector2I[2] array containing the two closest cells,
         * index 0 a Vector2I representing the closest cell in group1
         * index 1 a Vector2I representing the closest cell in group2
         *
         *
         */
        Vector2I closestCellGroup1 = group1[0];
        Vector2I closestCellGroup2 = group2[0];
        float minDistance = Single.PositiveInfinity;
        foreach (Vector2I group1Cell in group1)
        {
            foreach (Vector2I group2Cell in group2)
            {
                float currDistance = Math.Abs((group1Cell - group2Cell).Length());
                if (currDistance < minDistance)
                {
                    minDistance = currDistance;
                    closestCellGroup1 = group1Cell;
                    closestCellGroup2 = group2Cell;
                }
            }
        }

        Vector2I[] closestCells = new Vector2I[2] { closestCellGroup1, closestCellGroup2 };
        return closestCells;
    }

    private int[] getTwoClosestIslands()
    {
        float minDistanceIslandLength = Single.PositiveInfinity;
        int[] minDistanceIslands = new int[2];
        for (int i = 0; i < islands.Count; i++)
        {
            for (int j = i; j < islands.Count; j++)
            {
                Vector2I[] closestCells = getTwoClosestCells(islands[i], islands[j]);
                float currDistance = Math.Abs((closestCells[0] - closestCells[1]).Length());
                if (currDistance < minDistanceIslandLength)
                {
                    minDistanceIslandLength = currDistance;
                    minDistanceIslands[0] = i;
                    minDistanceIslands[1] = j;
                }
            }
        }

        return minDistanceIslands;
    }

    private void connectIslands()
    {
        while (islands.Count > 0)
        {
            int[] currIslandPairIndexes = getTwoClosestIslands();
            List<Vector2I> startIsland = islands[currIslandPairIndexes[0]];
            List<Vector2I> endIsland = islands[currIslandPairIndexes[1]];
            Vector2I[] currIslandPairClosestCells = getTwoClosestCells(startIsland, endIsland);

            drawPathBetweenIslands(currIslandPairClosestCells[0], currIslandPairClosestCells[1], 3);
            islands.Remove(startIsland);
            islands.Remove(endIsland);
        }
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
                if ((WorldDataStates)worldDataState == WorldDataStates.Grass && GD.Randf() < .3 &&
                    uniformNeighbors(row, col, 3))
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


        initWorldData();
        smoothWorldData(5);
        smoothWorldData(5);
        smoothWorldData(6);
        smoothWorldData(6);
        smoothWorldData(7);
        markShrinesWorldData();

        //printWorldData();
        //printWorldData();
        findAllIslands();
        // GD.Print("ISLANDS: ");
        // foreach (List<Vector2I> island in islands)
        // {
        //     string str = "";
        //     foreach (Vector2I vec in island)
        //     {
        //         str += "(" + vec.X + ", " + vec.Y + ")";
        //     }
        //     GD.Print(str);
        // }
        connectIslands();
        //printWorldData();
        populateMap();
    }
}