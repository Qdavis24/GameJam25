using System.Collections.Generic;
using System.Linq;
using Godot;
using System;

namespace GameJam25.scripts.world_generation.pipeline.logical_stages;

public partial class FindConnectionsStage : PipelineStage
{
    // Local Members to cache Global Data (Logical World)
    private List<Island> _allIslands;
    
    //Stage temp members
    private List<IslandEdge> _allIslandEdges;
    
    // Stage result members
    private List<IslandEdge> _validIslandEdges;
    public override void ProcessStage()
    {
        // cache needed references from Global Data
        _allIslands = World.LogicalData.Islands;
        
        // trigger stage logic 
        GenerateAllIslandEdges();
        GenerateValidConnections();
        
        // update Global Data
        World.LogicalData.IslandEdges = _validIslandEdges;
    }

    private void GenerateAllIslandEdges()
    {
        _allIslandEdges = new List<IslandEdge>();
        for (int island1 = 0; island1 < _allIslands.Count; island1++)
        {
            for (int island2 = island1 + 1; island2 < _allIslands.Count; island2++)
            {
                Vector2I distanceVec = _allIslands[island1].Centroid - _allIslands[island2].Centroid;
                float distance = distanceVec.LengthSquared();
                IslandEdge islandEdge = new IslandEdge(_allIslands[island1], _allIslands[island2], distance);
                _allIslandEdges.Add(islandEdge);
            }
        }

        _allIslandEdges = _allIslandEdges.OrderBy(edge => edge.Distance).ToList();
        
    }

    public void GenerateValidConnections()
    {
        _validIslandEdges = new List<IslandEdge>();
        UnionFind uf = new UnionFind(_allIslands.Count);
        foreach (IslandEdge edge in _allIslandEdges)
        {
            Island island1 = edge.Island1;
            Island island2 = edge.Island2;
            int island1Index = _allIslands.IndexOf(island1);
            int island2Index = _allIslands.IndexOf(island2);
            if (uf.Find(island1Index) != uf.Find(island2Index))
            {
                _validIslandEdges.Add(edge);
                uf.Union(island1Index, island2Index);
            }
        }
    }
    
    
}