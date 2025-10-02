using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Godot;

namespace GameJam25.scripts.world_generation
{
    public class WorldGenerator
    {
        public Matrix Map { get; }
        public List<Island> Islands { get; } = new List<Island>();
        public List<Shrine> Shrines { get; } = new List<Shrine>();

        public PathConfig PathConfiguration { get; }
        public ShrineConfig ShrineConfiguration { get; }

        private enum MapCellStates
        {
            Walkable,
            NonWalkable,
            Shrine
        }

        public WorldGenerator(int mapSizeX, int mapSizeY, int[] possibleStates, float[] stateSpawnWeights,
            int numSmooths, PathConfig pathConfig, ShrineConfig shrineConfig)
        {
            PathConfiguration = pathConfig;
            ShrineConfiguration = shrineConfig;

            Map = ProcWorldGeneration.InitWorldData(mapSizeX, mapSizeY, possibleStates, stateSpawnWeights);
            for (int smoothPass = 0; smoothPass < numSmooths; smoothPass++)
                Map = ProcWorldGeneration.SmoothWorldData(Map, possibleStates, 1, true);
            Islands = IslandConnector.FindIslands(Map, (int)MapCellStates.Walkable);
            IslandConnector.ConnectIslands(Map, Islands, PathConfiguration);
            markShrinesWorldData();
        }

        private Shrine createShrine(int row, int col)
        {
            List<Vector2I> allCells = new List<Vector2I>();
            for (int currRow = row; currRow < row + ShrineConfiguration.ShrineSizeY; currRow++)
            {
                for (int currCol = col; currCol < col + ShrineConfiguration.ShrineSizeX; currCol++)
                {
                    Vector2I currCell = new Vector2I(currRow, currCol);
                    allCells.Add(currCell);
                }
            }

            return new Shrine(allCells);
        }

        private void markShrinesWorldData()
        {
            Island[] shrineIslands = new Island[ShrineConfiguration.NumShrines];
            bool validIslands = false;
            while (validIslands != true)
            {
                shrineIslands = new Island[ShrineConfiguration.NumShrines];
                for (int i = 0; i < shrineIslands.Length; i++)
                    shrineIslands[i] = Islands[GD.RandRange(0, Islands.Count)];

                validIslands = true;
                for (int i = 0; i < shrineIslands.Length; i++)
                {
                    for (int j = i + 1; j < shrineIslands.Length; j++)
                    {
                        if ((shrineIslands[i].Centroid - shrineIslands[j].Centroid).Length() <
                            ShrineConfiguration.MinDistance)
                        {
                            validIslands = false;
                        }
                    }
                }
            }

            for (int i = 0; i < ShrineConfiguration.NumShrines; i++)
            {
                Shrine currShrine = createShrine(shrineIslands[i].Centroid.Y, shrineIslands[i].Centroid.X);
                Shrines.Add(currShrine);
                MatrixUtils.InsertIsland(Map, currShrine.AllCells, (int)MapCellStates.Shrine);
            }
        }
    }
}