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
            GD.Print(Islands.Count);
            IslandConnector.ConnectIslands(Map, Islands, PathConfiguration);
            markShrinesWorldData();
        }

        private Shrine createShrine(Vector2I rootCell)
        {
            List<Vector2I> allCells = new List<Vector2I>();
            int rowDirection = rootCell.X > ShrineConfiguration.ShrineSizeX ? -1 : 1;
            int colDirection = rootCell.Y > ShrineConfiguration.ShrineSizeY ? -1 : 1;
            for (int rowShift = 0; rowShift < ShrineConfiguration.ShrineSizeY; rowShift++)
            {
                for (int colShift = 0; colShift < ShrineConfiguration.ShrineSizeX; colShift++)
                {
                    int currRow = rootCell.X + (rowShift * rowDirection);
                    int currCol = rootCell.Y + (colShift * colDirection);
                    Vector2I currCell = new Vector2I(currRow, currCol);
                    allCells.Add(currCell);
                }
            }

            return new Shrine(allCells, rootCell, rowDirection, colDirection);
        }

        private void markShrinesWorldData()
        {
            List<Shrine> shrines = new List<Shrine>();
            int count = 0;
            while (shrines.Count < ShrineConfiguration.NumShrines && count < 100)
            {
                count++;
                Vector2I possibleShrinePlacement = Islands[GD.RandRange(0, Islands.Count - 1)].Centroid;
                bool validPlacement = true;
                foreach (Shrine currShrine in shrines)
                {
                    if ((possibleShrinePlacement - currShrine.RootCell).Length() < ShrineConfiguration.MinDistance)
                    {
                        validPlacement = false;
                        break;
                    }
                }

                if (validPlacement)
                {
                    Shrine newShrine = createShrine(possibleShrinePlacement);
                    shrines.Add(newShrine);
                }
            }

            foreach(Shrine currShrine in shrines)
            {
                MatrixUtils.InsertIsland(Map, currShrine.AllCells, (int)MapCellStates.Shrine);
                Shrines.Add(currShrine);
            }
        }

        
    }
}