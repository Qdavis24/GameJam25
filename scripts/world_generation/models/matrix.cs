
namespace GameJam25.scripts.world_generation.models
{
    public class Matrix(int[,] array)
    {
        public int[,] Array { get; private set; } = array;
        public int RowLength { get; private set; } = array.GetLength(0);
        public int ColLength { get; private set; } = array.GetLength(1);
    }
}