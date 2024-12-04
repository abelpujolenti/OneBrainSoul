using System;

namespace Serialize.NavMeshGraph
{
    [Serializable]
    public class SerializableEdge
    {
        public uint toNodeIndex;
        public float cost;
    }
}