using Godot;
using System.Collections.Generic;
using System;

namespace GameJam25.scripts.world_generation.utilities
{
    
    
    public static class MatrixUtils
    {
        public static int[] WraparoundIndexes(int arrayRowSize, int arrayColSize, int col, int row)
        {
            if (col < 0) col += arrayColSize;
            if (col >= arrayColSize) col -= arrayColSize;
            if (row < 0) row += arrayRowSize;
            if (row >= arrayRowSize) row -= arrayRowSize;

            return new int[] { col, row };
        }

        public static bool UniformNeighbors(int[,] matrix, int col, int row,
            int neighborDepth, bool wrapAround)
        {
            for (int colShift = -neighborDepth; colShift <= neighborDepth; colShift++)
            {
                for (int rowShift = -neighborDepth; rowShift <= neighborDepth; rowShift++)
                {
                    int currCol = col + colShift;
                    int currRow = row + rowShift;
                    if (wrapAround)
                    {
                        int[] wrappedColRow =
                            WraparoundIndexes(matrix.GetLength(1), matrix.GetLength(0), currCol, currRow);
                        currCol = wrappedColRow[0];
                        currRow = wrappedColRow[1];
                    }
                    else
                    {
                        if (currCol < 0 || currCol >= matrix.GetLength(0) || currRow < 0 || currRow >= matrix.GetLength(1)) continue;
                    }

                    if (currCol == col && currRow == row) continue;

                    if (matrix[currCol, currRow] != matrix[col, row]) return false;
                }
            }

            return true;
        }

        public static void SetNeighbors(int[,] matrix, int col, int row,
            int neighborDepth,
            int state,
            bool wrapAround)
        {
            for (int colShift = -neighborDepth; colShift <= neighborDepth; colShift++)
            {
                for (int rowShift = -neighborDepth; rowShift <= neighborDepth; rowShift++)
                {
                    int currCol = col + colShift;
                    int currRow = row + rowShift;
                    if (wrapAround)
                    {
                        int[] wrappedColRow =
                            WraparoundIndexes(matrix.GetLength(1), matrix.GetLength(0), currCol, currRow);
                        currCol = wrappedColRow[0];
                        currRow = wrappedColRow[1];
                    }
                    else
                    {
                        if (currCol < 0 || currCol >= matrix.GetLength(0) || currRow < 0 || currRow >= matrix.GetLength(1)) continue;
                    }

                    matrix[currCol, currRow] = state;
                }
            }
        }

        public static int MajorityNeighbor(int[,] matrix, int[] cellStates, int col,
            int row,
            int neighborDepth,
            bool wrapAround)
        {
            int[] counts = new int[cellStates.Length];
            for (int colShift = -neighborDepth; colShift <= neighborDepth; colShift++)
            {
                for (int rowShift = -neighborDepth; rowShift <= neighborDepth; rowShift++)
                {
                    int currCol = col + colShift;
                    int currRow = row + rowShift;
                    if (wrapAround)
                    {
                        int[] wrappedColRow =
                            WraparoundIndexes(matrix.GetLength(1), matrix.GetLength(0), currCol, currRow);
                        currCol = wrappedColRow[0];
                        currRow = wrappedColRow[1];
                    }
                    else
                    {
                        if (currCol < 0 || currCol >= matrix.GetLength(0) || currRow < 0 || currRow >= matrix.GetLength(1)) continue;
                    }

                    counts[matrix[currCol, currRow]]++;
                }
            }

            int currMaxState = 0;
            int currMaxCount = 0;
            for (int state = 0; state < cellStates.Length; state++)
            {
                if (counts[state] > currMaxCount)
                {
                    currMaxCount = counts[state];
                    currMaxState = state;
                }
            }

            return currMaxState;
        }

        public static void InsertIsland(int[,] matrix, List<Vector2I> allCells, int state)
        {
            foreach (Vector2I cell in allCells)
            {
                matrix[cell.X, cell.Y] = state;
            }
        }
        public static void PrintMatrix(int[,] matrix)
        {
            for (int row = 0; row < matrix.GetLength(1); row++)
            {
                string curr_row = "";
                for (int col = 0; col < matrix.GetLength(0); col++)
                {
                    curr_row += matrix[col, row];
                }

                GD.Print(curr_row);
            }
        }
    }
}