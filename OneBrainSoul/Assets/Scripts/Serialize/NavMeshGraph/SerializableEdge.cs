using MessagePack;

namespace Serialize.NavMeshGraph
{
    [MessagePackObject]
    public class SerializableEdge
    {
        [Key(0)]
        public uint toNodeIndex;
        
        [Key(1)]
        public bool isAJump;
        
        [Key(2)]
        public float cost;
        
        [Key(3)]
        public float baseCostMultiplier;
    }
}