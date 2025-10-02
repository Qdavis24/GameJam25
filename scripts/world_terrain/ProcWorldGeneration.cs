using System;
using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_terrain
{
    public static class ProcWorldGeneration
    {
        public static Matrix InitWorldData(int arrayRowSize, int arrayColSize, int[] cellStates,
            float[] cellStatesProbabilities)
        {
            int[,] array = new int[arrayRowSize, arrayColSize];
            for (int row = 0; row < arrayRowSize; row++)
            {
                for (int col = 0; col < arrayColSize; col++)
                {
                    float rInt = GD.Randf();
                    float cummulative = 0f;
                    for (int currState = 0; currState < cellStates.Length; currState++)
                    {
                        cummulative += cellStatesProbabilities[currState];
                        if (rInt <= cummulative)
                        {
                            array[row, col] = cellStates[currState];
                            break;
                        }
                    }
                }
            }

            return new Matrix(array);
        }

        public static Matrix SmoothWorldData(Matrix matrix, int[] cellStates,
            int neighborDepth, bool wrapAround)
        {
            int[,] copyArray = (int[,])matrix.Array.Clone();
            for (int row = 0; row < matrix.RowLength; row++)
            {
                for (int col = 0; col < matrix.ColLength; col++)
                {
                    int newState =
                        MatrixUtils.MajorityNeighbor(matrix, cellStates, row, col, neighborDepth, wrapAround);
                    copyArray[row, col] = newState;
                }
            }

            return new Matrix(copyArray);
        }
    
        
    }
    
}