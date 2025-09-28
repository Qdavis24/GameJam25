using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WorldTerrain : Node2D
{
    [ExportCategory("World Generation")]
    [Export] int MapSizeRows;
    [Export] int MapSizeCols;
    [Export] int[] PossibleStates; // All possible values that can exist in world cells (e.g., grass, water, mountain)
    [Export] float[] StateSpawnWeights; // Probability weights for each state during initial world generation (matched indexing)
    [Export] Curve PathCurve; // shape of path
    [Export] int PathCurveSize; // Magnitude of path curves

    private enum WorldDataStates
    {
        Walkable,
        NonWalkable,
    };


    private List<List<Vector2I>> islands = new List<List<Vector2I>>();
    private bool[,] islandVisitedCells;

    [ExportCategory("Tiles")]
    [Export] TileMapLayer BaseTileMapLayer;
    [Export] TileMapLayer ObjectTileMapLayer;
    [Export] TileConfig TileConfiguration;

    [ExportCategory("Shrines")]
    [Export] int ShrineSizeRows;
    [Export] int ShrineSizeCols;
    [Export] int MinimumDistanceShrines; // in tile cells
    [Export] private PackedScene[] allShrinePckdScns; // Racoon index 0 , Own index 1, Rabbit index 2

    private List<Vector2I> shrinesSpawnCoord = new List<Vector2I>(); // where we are going to spawn each shrine (top left cell) Vector2I (col, row)

    private int[,] worldData; // array to represent how to build our map

    private int wrapIndexes(int index, int axisLength)
    {
        if (index < 0) index += axisLength;
        if (index >= axisLength) index -= axisLength;

        return index;
    }
    private void setNeighbors(int sampleRow, int sampleCol, WorldDataStates state, int numNeighbors)
    {
        for (int rowShift = -numNeighbors; rowShift < numNeighbors; rowShift++)
        {
            for (int colShift = -numNeighbors; colShift < numNeighbors; colShift++)
            {
                int row = wrapIndexes(sampleRow + rowShift, MapSizeRows);
                int col = wrapIndexes(sampleCol + colShift, MapSizeCols);
                if (row == sampleRow && col == sampleCol) continue;
                worldData[row, col] = (int)state;
            }
        }
    }

    private bool uniformNeighbors(int sampleRow, int sampleCol, int numNeighbors)
    {
        for (int rowShift = -numNeighbors; rowShift <= numNeighbors; rowShift++)
        {
            for (int colShift = -numNeighbors; colShift <= numNeighbors; colShift++)
            {
                int row = wrapIndexes(sampleRow + rowShift, MapSizeRows);
                int col = wrapIndexes(sampleCol + colShift, MapSizeCols);

                if (row == sampleRow && col == sampleCol) continue;

                if (worldData[row, col] != worldData[sampleRow, sampleCol]) return false;
            }
        }
        return true;

    }

    private int majorityNeighbor(int sampleRow, int sampleCol, int numNeighbors)
    {
        int[] counts = new int[PossibleStates.Length];

        for (int rowShift = -numNeighbors; rowShift <= numNeighbors; rowShift++)
        {
            for (int colShift = -numNeighbors; colShift <= numNeighbors; colShift++)
            {
                int row = wrapIndexes(sampleRow + rowShift, MapSizeRows);
                int col = wrapIndexes(sampleCol + colShift, MapSizeCols);
                if (row == sampleRow && col == sampleCol) continue;

                counts[worldData[row, col]] += 1;
            }
        }
        int currMaxState = 0;
        int currMaxCount = 0;
        for (int state = 0; state < counts.Length; state++)
        {
            if (counts[state] > currMaxCount)
            {
                currMaxCount = counts[state];
                currMaxState = state;
            }
        }
        return currMaxState;
    }
    private void initWorldData()
    {
        for (int row = 0; row < MapSizeRows; row++)
        {
            for (int col = 0; col < MapSizeCols; col++)
            {
                float rInt = GD.Randf();
                float cummulative = 0f;
                for (int k = 0; k < PossibleStates.Length; k++)
                {
                    cummulative += StateSpawnWeights[k];
                    if (rInt <= cummulative)
                    {
                        worldData[row, col] = PossibleStates[k];
                        break;
                    }
                }
            }
        }
    }

    private void smoothWorldData()
    {
        int[,] copyTerrain = (int[,])worldData.Clone();

        for (int row = 0; row < MapSizeRows; row++)
        {
            for (int col = 0; col < MapSizeCols; col++)
            {

                int newState = majorityNeighbor(row, col, 1);
                copyTerrain[row, col] = newState;

            }
        }
        worldData = copyTerrain;
    }

    private void dfsRecordIsland(int row, int col, WorldDataStates islandState)
    {
        row = wrapIndexes(row, MapSizeRows);
        col = wrapIndexes(col, MapSizeCols);
        
        

        if (islandVisitedCells[row, col]) return;
        islandVisitedCells[row, col] = true;
        if (worldData[row, col] == (int)islandState) islands[islands.Count - 1].Add(new Vector2I(row, col));
        else return;
        dfsRecordIsland(row, col - 1, islandState); // left
        dfsRecordIsland(row, col + 1, islandState); // right
        dfsRecordIsland(row - 1, col, islandState); // up
        dfsRecordIsland(row + 1, col, islandState); // down
        dfsRecordIsland(row - 1, col - 1, islandState); // up-left
        dfsRecordIsland(row - 1, col + 1, islandState); // up-right
        dfsRecordIsland(row + 1, col - 1, islandState); // down-left
        dfsRecordIsland(row + 1, col + 1, islandState); // down-right
    }

    private void findAllIslands(WorldDataStates islandState)
    {
        islandVisitedCells = new bool[MapSizeRows, MapSizeCols];
        islands = new List<List<Vector2I>>();
        for (int row = 0; row < MapSizeRows; row++)
        {
            for (int col = 0; col < MapSizeCols; col++)
            {
                if (worldData[row, col] == (int)islandState && islandVisitedCells[row, col] != true)
                {
                    List<Vector2I> currIsland = new List<Vector2I>();
                    islands.Add(currIsland);
                    dfsRecordIsland(row, col, islandState);
                }
            }
        }
    }


    private void drawPathBetweenIslands(Vector2I startCoord, Vector2I endCoord, WorldDataStates pathState, int pathRadius)
    {

        Vector2 direction = endCoord - startCoord;
        // Calculate total steps needed
        int totalSteps = (int)Math.Max(Math.Abs(direction.X), Math.Abs(direction.Y));
        if (totalSteps == 0) return; // Same position

        // Get perpendicular direction for curve offset
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X).Normalized();



        for (int step = 0; step <= totalSteps; step++)
        {
            float progress = (float)step / totalSteps; // 0 to 1

            // Get straight line position
            Vector2 straightPos = startCoord + direction * progress;

            // Sample curve for bend amount
            float bendAmount = PathCurve.Sample(progress);

            // Apply perpendicular offset
            Vector2 curvedPos = straightPos + perpendicular * bendAmount * PathCurveSize;

            // Convert to grid coordinates
            int row = (int)Math.Round(curvedPos.X);
            int col = (int)Math.Round(curvedPos.Y);

            // Place the path tile
            setNeighbors(row, col, pathState, pathRadius);
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
                float currDistance = (group1Cell - group2Cell).Length();
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
        for (int island1 = 0; island1 < islands.Count; island1++)
        {
            for (int island2 = island1 + 1; island2 < islands.Count; island2++)
            {
                Vector2I[] closestCells = getTwoClosestCells(islands[island1], islands[island2]);
                float currDistance = (closestCells[0] - closestCells[1]).Length();
                if (currDistance < minDistanceIslandLength)
                {
                    minDistanceIslandLength = currDistance;
                    minDistanceIslands[0] = island1;
                    minDistanceIslands[1] = island2;
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
            drawPathBetweenIslands(currIslandPairClosestCells[0], currIslandPairClosestCells[1], WorldDataStates.Walkable, 2);
            islands.Remove(startIsland);
            islands.Remove(endIsland);
        }
    }


    private void markShrinesWorldData()
    {
        // Pick N sub 2d arrays in mapData with minimum distance apart (N = number of shrine types)
        // CAREFUL too big min distance can create inf loop here.
        List<Vector2I> shrinePlacementMapDataCoord = new List<Vector2I>(); // X is COl Y is ROW
        while (shrinePlacementMapDataCoord.Count < allShrinePckdScns.Length)
        {
            Vector2I newCoord = new Vector2I(
                GD.RandRange(0, MapSizeRows - ShrineSizeRows),
                GD.RandRange(0, MapSizeCols - ShrineSizeCols));
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
                int row = newCoord.X;
                int col = newCoord.Y;
                for (int shrineCellRow = row; shrineCellRow < row + ShrineSizeRows; shrineCellRow++)
                {
                    for (int shrineCellCol = col; shrineCellCol < col + ShrineSizeCols; shrineCellCol++)
                    {
                        worldData[shrineCellRow, shrineCellCol] = -1;
                    }
                }

                shrinesSpawnCoord.Add(new Vector2I(row, col));
            }
        }
    }

    private void printWorldData()
    {
        for (int row = 0; row < worldData.GetLength(0); row++)
        {
            string curr_row = "";
            for (int col = 0; col < worldData.GetLength(1); col++)
            {
                curr_row += worldData[row, col];
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
                Vector2I[] tileOptions = new Vector2I[] { };
                switch ((WorldDataStates)worldDataState)
                {
                    case WorldDataStates.Walkable:
                        tileOptions = TileConfiguration.BaseLayerWalkableTilesAtlasCoords;
                        break;
                    case WorldDataStates.NonWalkable:
                        tileOptions = TileConfiguration.BaseLayerNonWalkableTilesAtlasCoords;
                        break;
                }
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
                if (worldDataState == -1 || GD.Randf() > .5) continue;
                Vector2I[] tileOptions = new Vector2I[] { };
                switch ((WorldDataStates)worldDataState)
                {
                    case WorldDataStates.Walkable:
                        tileOptions = TileConfiguration.ObjectLayerWalkableTilesAtlasCoords;
                        break;
                    case WorldDataStates.NonWalkable:
                        tileOptions = TileConfiguration.ObjectLayerNonWalkableTilesAtlasCoords;
                        break;
                }
                ObjectTileMapLayer.SetCell(new Vector2I(row, col), 0, tileOptions[GD.Randi() % tileOptions.Length]);
            }
        }
    }

    private void spawnShrines()
    {
        for (int shrine = 0; shrine < allShrinePckdScns.Length; shrine++)
        {
            Node2D currShrine = allShrinePckdScns[shrine].Instantiate<Node2D>();
            Vector2I centerTile = new Vector2I(
                shrinesSpawnCoord[shrine].X + (ShrineSizeRows / 2),
                shrinesSpawnCoord[shrine].Y + (ShrineSizeCols / 2)
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
        for (int row = 0; row < worldData.GetLength(0); row++)
        {
            for (int col = 0; col < worldData.GetLength(1); col++)
            {
                ObjectTileMapLayer.EraseCell(new Vector2I(row, col));
                BaseTileMapLayer.EraseCell(new Vector2I(row, col));
            }
        }
    }

    public override void _Ready()
    {

        worldData = new int[MapSizeRows, MapSizeCols];

        initWorldData();
        smoothWorldData();
        smoothWorldData();
        // smoothWorldData();
        // smoothWorldData();
        // smoothWorldData();
        markShrinesWorldData();
        // findAllIslands(WorldDataStates.Walkable);
        // connectIslands();
        populateMap();

    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("regen"))
        {
            wipeMap();
            findAllIslands(WorldDataStates.Walkable);
            GD.Print(islands.Count);
            //connectIslands();
            populateMap();
        }
    }


}