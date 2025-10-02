using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GameJam25.scripts.world_terrain
{
    public static class IslandConnector
    {
        private static void dfsFloodFill(Matrix matrix, int row, int col, int state, List<Vector2I> allCells,
            List<Vector2I> borderCells, bool[,] visited)
        {
            if (row < 0 || row >= matrix.RowLength || col < 0 || col >= matrix.ColLength || visited[row, col]) return;

            visited[row, col] = true;
            if (matrix.Array[row, col] == state) allCells.Add(new Vector2I(col, row));
            else return;
            dfsFloodFill(matrix, row, col - 1, state, allCells, borderCells, visited); // left
            dfsFloodFill(matrix, row, col + 1, state, allCells, borderCells, visited); // right
            dfsFloodFill(matrix, row - 1, col, state, allCells, borderCells, visited); // up
            dfsFloodFill(matrix, row + 1, col, state, allCells, borderCells, visited); // down
            dfsFloodFill(matrix, row - 1, col - 1, state, allCells, borderCells, visited); // up-left
            dfsFloodFill(matrix, row - 1, col + 1, state, allCells, borderCells, visited); // up-right
            dfsFloodFill(matrix, row + 1, col - 1, state, allCells, borderCells, visited); // down-left
            dfsFloodFill(matrix, row + 1, col + 1, state, allCells, borderCells, visited); // down-right
        }

        public static List<Island> FindIslands(Matrix matrix, int state)
        {
            List<Island> allIslands = new List<Island>();
            bool[,] visited = new bool[matrix.RowLength, matrix.ColLength];
            for (int row = 0; row < matrix.RowLength; row++)
            {
                for (int col = 0; col < matrix.ColLength; col++)
                {
                    List<Vector2I> allCells = new List<Vector2I>();
                    List<Vector2I> borderCells = new List<Vector2I>();
                    dfsFloodFill(matrix, row, col, state, allCells, borderCells, visited);
                    if (allCells.Count > 0)
                    {
                        Island island = new Island(allCells);
                        allIslands.Add(island);
                    }
                }
            }

            return allIslands;
        }

        private static List<IslandEdge> generateAllIslandEndges(List<Island> allIslands)
        {
            List<IslandEdge> islandEdges = new List<IslandEdge>();
            for (int island1 = 0; island1 < allIslands.Count; island1++)
            {
                for (int island2 = island1 + 1; island2 < allIslands.Count; island2++)
                {
                    Vector2I distanceVec = allIslands[island1].Centroid - allIslands[island2].Centroid;
                    float distance = distanceVec.LengthSquared();
                    IslandEdge islandEdge = new IslandEdge(allIslands[island1], allIslands[island2], distance);
                    islandEdges.Add(islandEdge);
                }
            }

            return islandEdges.OrderBy(edge => edge.Distance).ToList();
        }

        public static void ConnectIslands(Matrix matrix, int state, int pathRadius, int pathCurveSize, Curve pathCurve)
        {
            List<Island> allIslands = FindIslands(matrix, state);
            List<IslandEdge> islandEdges = generateAllIslandEndges(allIslands);
            UnionFind uf = new UnionFind(allIslands.Count);
            foreach (IslandEdge edge in islandEdges)
            {
                Island island1 = edge.Island1;
                Island island2 = edge.Island2;
                int island1Index = allIslands.IndexOf(island1);
                int island2Index = allIslands.IndexOf(island2);
                if (uf.Find(island1Index) != uf.Find(island2Index))
                {
                    MatrixUtils.DrawPathBetweenCells(matrix, island1.Centroid, island2.Centroid, pathRadius,
                        pathCurveSize, pathCurve);
                    uf.Union(island1Index, island2Index);
                }
            }
        }
    }
}