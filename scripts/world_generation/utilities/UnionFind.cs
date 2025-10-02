namespace GameJam25.scripts.world_generation.utilities
{
    public class UnionFind
    {
        private int[] parents;
        private int[] ranks;

        public UnionFind(int length)
        {
            parents = new int[length];
            ranks = new int[length];
            for (int i = 0; i < length; i++)
            {
                parents[i] = i;
            }
        }

        public int Find(int x)
        {
            if (parents[x] == x) return x;
            parents[x] = Find(parents[x]);
            return parents[x];
        }

        public void Union(int x, int y)
        {
            int xParent = Find(x);
            int yParent = Find(y);
            if (xParent == yParent) return;
            if (ranks[xParent] == ranks[yParent])
            {
                parents[xParent] = yParent;
                ranks[yParent]++;
            }
            else if (ranks[xParent] > ranks[yParent])
            {
                parents[yParent] = xParent;
            }
            else if (ranks[xParent] < ranks[yParent])
            {
                parents[xParent] = yParent;
            }
            
        }
    }
}