using Godot;
using System.Collections.Generic;
using System;

namespace GameJam25.scripts.world_generation.utilities
{
    
    
    public static class MatrixUtils
    {
        public static int[] WraparoundIndexes(int arrayRowSize, int arrayColSize, int row, int col)
        {
            if (row < 0) row += arrayRowSize;
            if (row >= arrayRowSize) row -= arrayRowSize;
            if (col < 0) col += arrayColSize;
            if (col >= arrayColSize) col -= arrayColSize;

            return new int[] { row, col };
        }

        public static bool UniformNeighbors(int[,] matrix, int row, int col,
            int neighborDepth, bool wrapAround)
        {
            for (int rowShift = -neighborDepth; rowShift <= neighborDepth; rowShift++)
            {
                for (int colShift = -neighborDepth; colShift <= neighborDepth; colShift++)
                {
                    int currRow = row + rowShift;
                    int currCol = col + colShift;
                    if (wrapAround)
                    {
                        int[] wrappedRowCol =
                            WraparoundIndexes(matrix.GetLength(0), matrix.GetLength(1), currRow, currCol);
                        currRow = wrappedRowCol[0];
                        currCol = wrappedRowCol[1];
                    }
                    else
                    {
                        if (currRow < 0 || currRow >= matrix.GetLength(0) || currCol < 0 || currCol >= matrix.GetLength(1)) continue;
                    }

                    if (currRow == row && currCol == col) continue;

                    if (matrix[currRow, currCol] != matrix[row, col]) return false;
                }
            }

            return true;
        }

        public static void SetNeighbors(int[,] matrix, int row, int col,
            int neighborDepth,
            int state,
            bool wrapAround)
        {
            for (int rowShift = -neighborDepth; rowShift <= neighborDepth; rowShift++)
            {
                for (int colShift = -neighborDepth; colShift <= neighborDepth; colShift++)
                {
                    int currRow = row + rowShift;
                    int currCol = col + colShift;
                    if (wrapAround)
                    {
                        int[] wrappedRowCol =
                            WraparoundIndexes(matrix.GetLength(0), matrix.GetLength(1), currRow, currCol);
                        currRow = wrappedRowCol[0];
                        currCol = wrappedRowCol[1];
                    }
                    else
                    {
                        if (currRow < 0 || currRow >= matrix.GetLength(0) || currCol < 0 || currCol >= matrix.GetLength(1)) continue;
                    }

                    matrix[currRow, currCol] = state;
                }
            }
        }

        public static int MajorityNeighbor(int[,] matrix, int[] cellStates, int row,
            int col,
            int neighborDepth,
            bool wrapAround)
        {
            int[] counts = new int[cellStates.Length];
            for (int rowShift = -neighborDepth; rowShift <= neighborDepth; rowShift++)
            {
                for (int colShift = -neighborDepth; colShift <= neighborDepth; colShift++)
                {
                    int currRow = row + rowShift;
                    int currCol = col + colShift;
                    if (wrapAround)
                    {
                        int[] wrappedRowCol =
                            WraparoundIndexes(matrix.GetLength(0), matrix.GetLength(1), currRow, currCol);
                        currRow = wrappedRowCol[0];
                        currCol = wrappedRowCol[1];
                    }
                    else
                    {
                        if (currRow < 0 || currRow >= matrix.GetLength(0) || currCol < 0 || currCol >= matrix.GetLength(1)) continue;
                    }

                    counts[matrix[currRow, currCol]]++;
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
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                string curr_row = "";
                for (int col = 0; col < matrix.GetLength(1); col++)
                {
                    curr_row += matrix[row, col];
                }

                GD.Print(curr_row);
            }
        }
    }
}