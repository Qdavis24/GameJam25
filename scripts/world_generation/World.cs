using System.Collections.Generic;
namespace GameJam25.scripts.world_generation;

public class World
{
    public Matrix Map { get; }
    public List<Island> Islands { get; } = new List<Island>();
    public List<Shrine> Shrines { get; } = new List<Shrine>();
}