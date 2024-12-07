using MessagePack;

namespace Serialize.NavMeshGraph
{
    [MessagePackObject]
    public class SerializableEdge
    {
        [Key(0)]
        public uint toNodeIndex;
        
        [Key(1)]
        public float cost;
        
        [Key(2)]
        public float baseCostMultiplier;
    }
}