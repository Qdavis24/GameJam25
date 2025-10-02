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

        public static bool UniformNeighbors(Matrix matrix, int row, int col,
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
                            WraparoundIndexes(matrix.RowLength, matrix.ColLength, currRow, currCol);
                        currRow = wrappedRowCol[0];
                        currCol = wrappedRowCol[1];
                    }
                    else
                    {
                        if (currRow < 0 || currRow >= matrix.RowLength || currCol < 0 || currCol >= matrix.ColLength) continue;
                    }

                    if (currRow == row && currCol == col) continue;

                    if (matrix.Array[currRow, currCol] != matrix.Array[row, col]) return false;
                }
            }

            return true;
        }

        public static void SetNeighbors(Matrix matrix, int row, int col,
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
                            WraparoundIndexes(matrix.RowLength, matrix.ColLength, currRow, currCol);
                        currRow = wrappedRowCol[0];
                        currCol = wrappedRowCol[1];
                    }
                    else
                    {
                        if (currRow < 0 || currRow >= matrix.RowLength || currCol < 0 || currCol >= matrix.ColLength) continue;
                    }

                    matrix.Array[currRow, currCol] = state;
                }
            }
        }

        public static int MajorityNeighbor(Matrix matrix, int[] cellStates, int row,
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
                            WraparoundIndexes(matrix.RowLength, matrix.ColLength, currRow, currCol);
                        currRow = wrappedRowCol[0];
                        currCol = wrappedRowCol[1];
                    }
                    else
                    {
                        if (currRow < 0 || currRow >= matrix.RowLength || currCol < 0 || currCol >= matrix.ColLength) continue;
                    }

                    counts[matrix.Array[currRow, currCol]]++;
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

        public static void DrawPathBetweenCells(Matrix matrix, Vector2I startCell,
            Vector2I endCell, PathConfig pathConfig)
        {
            Vector2 direction = endCell - startCell;

            // Calculate total steps needed
            int totalSteps = (int)Math.Max(Math.Abs(direction.X), Math.Abs(direction.Y));
            if (totalSteps == 0) return; // Same position

            // Get perpendicular direction for curve offset
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X).Normalized();

            int state = matrix.Array[endCell.Y, endCell.X];

            for (int step = 0; step <= totalSteps; step++)
            {
                float progress = (float)step / totalSteps; // 0 to 1

                // Get straight line position
                Vector2 straightPos = startCell + direction * progress;

                // Sample curve for bend amount
                float bendAmount = pathConfig.pathCurve.Sample(progress);

                // Apply perpendicular offset
                Vector2 curvedPos = straightPos + perpendicular * bendAmount * pathConfig.pathCurveSize;

                // Convert to grid coordinates
                int row = (int)Math.Round(curvedPos.Y);
                int col = (int)Math.Round(curvedPos.X);

                // Place the path tile
                SetNeighbors(matrix, row, col, pathConfig.PathRadius, state, true);
            }
        }

        public static void InsertIsland(Matrix matrix, List<Vector2I> allCells, int state)
        {
            foreach (Vector2I cell in allCells)
            {
                matrix.Array[cell.X, cell.Y] = state;
            }
        }
        public static void PrintMatrix(Matrix matrix)
        {
            for (int row = 0; row < matrix.RowLength; row++)
            {
                string curr_row = "";
                for (int col = 0; col < matrix.ColLength; col++)
                {
                    curr_row += matrix.Array[row, col];
                }

                GD.Print(curr_row);
            }
        }
    }
}