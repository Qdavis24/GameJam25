using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public class Island
    {
        public List<Vector2I> AllCells;
        public List<Vector2I> BorderCells;
        public Vector2I Centroid { get; private set; }

        public Island()
        {
            AllCells = new List<Vector2I>();
            BorderCells = new List<Vector2I>();
        }


        public void CalculateCentroid()
        {
            Vector2I sum = new Vector2I();
            foreach (Vector2I cell in BorderCells)
            {
                sum += cell;
            }
            Centroid = sum / BorderCells.Count;
        }
    }
}