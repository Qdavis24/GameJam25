using Godot;
using System.Collections.Generic;

namespace GameJam25.scripts.world_generation.models
{
    public class Island
    {
        public List<Vector2I> AllCells { get; private set; }
        public List<Vector2I> BorderCells { get; private set; }
        public Vector2I Centroid { get; private set; }

        public Island(List<Vector2I> allCells)
        {
            AllCells = allCells;
            calculateCentroid();
        }


        private void calculateCentroid()
        {
            Vector2I sum = new Vector2I();
            foreach (Vector2I cell in AllCells)
            {
                sum += cell;
            }
            Centroid = sum / AllCells.Count;;
        }
    }
}