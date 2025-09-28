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

    private List<Vector2I> dfsFindIslandPaths(Vector2I currCell, List<Vector2I> targetIsland, int maxLength)
    {
        if (currCell.Y < 0 || currCell.Y >= worldData.GetLength(0) || currCell.X < 0 ||
            currCell.X >= worldData.GetLength(1) ||
            pathVisitedCells[currCell.Y, currCell.X] || maxLength <= 0) return null;
        pathVisitedCells[currCell.Y, currCell.X] = true;
        GD.Print($"Checking if {currCell} is in target island");
        GD.Print($"First target cell: {targetIsland[0]}");
        GD.Print($"Are they equal? {currCell.Equals(targetIsland[0])}");
        if (targetIsland.Contains(currCell)) return new List<Vector2I>() { currCell };
        List<Vector2I> path;
        Vector2I[] directions =
        {
            new Vector2I(-1, -1), // Up-left
            new Vector2I(0, -1), // Up
            new Vector2I(1, -1), // Up-right
            new Vector2I(-1, 0), // Left
            new Vector2I(1, 0), // Right
            new Vector2I(-1, 1), // Down-left
            new Vector2I(0, 1), // Down
            new Vector2I(1, 1) // Down-right
        };
        // Shuffle the directions array first
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Math.Abs((int)GD.Randi() % (i + 1));  // Add the +1 here
            Vector2I temp = directions[i];
            GD.Print(i,j);
            directions[i] = directions[j];
            directions[j] = temp;
        }

// Then try each direction in the shuffled order
        foreach (Vector2I dir in directions)
        {
            path = dfsFindIslandPaths(currCell + dir, targetIsland, maxLength - 1);
            if (path != null)
            {
                path.Insert(0, currCell);
                return path;
            }
        }


        return null;
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

    private void drawPathBetweenIslands(Vector2I island1ColRow, Vector2I island2ColRow)
    {
        Vector2I stepsNeeded = island1ColRow - island2ColRow;
        int rowStepsLeft = stepsNeeded.Y;
        int colStepsLeft = stepsNeeded.X;
        int rowStepDir = Math.Sign(rowStepsLeft);
        int colStepDir = Math.Sign(colStepsLeft);
        int currRow = island2ColRow.Y;
        int currCol = island2ColRow.X;
        int islandState = worldData[currRow, currCol];
        while (rowStepsLeft != 0 || colStepsLeft != 0)
        {
            // col steps left = -10 | 10
            // col dir = -1 | 1
            // curr col step = -1 | 1
            // col steps left = -10 - -1 = -9 | 10 - 1 = 9
            // curr col = curr col + -1 | curr col + 1
            int currRowStep = 0;
            int currColStep = 0;
            if (rowStepsLeft != 0) currRowStep = rowStepDir;
            if (colStepsLeft != 0) currColStep = colStepDir;
            rowStepsLeft -= currRowStep;
            colStepsLeft -= currColStep;
            currRow += currRowStep;
            currCol += currColStep;

            setNeighbors(currRow, currCol, islandState, 1);
        }
    }

    private void connectIslands()
    {
        GD.Print("island count: ");
        GD.Print(islands.Count);
        for (int i = 1; i < islands.Count; i++)
        {
            pathVisitedCells = new bool[MapSizeCols, MapSizeRows];
            Vector2I island1Cell = islands[i - 1][0];
            List<Vector2I> island2Cells = islands[i];
            int cellState = worldData[island1Cell.Y, island1Cell.X];
            GD.Print("Drawing path num: ");
            GD.Print(i);
            List<Vector2I> pathCells = null;
            while (pathCells == null)
            {
                pathCells = dfsFindIslandPaths(island1Cell, island2Cells, 1000);
            }
            foreach (Vector2I cell in pathCells)
            {
                setNeighbors(cell.Y, cell.X, cellState, 2);
            }
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
                if ((WorldDataStates)worldDataState == WorldDataStates.Grass && GD.Randf() < .5 &&
                    uniformNeighbors(row, col, 1))
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