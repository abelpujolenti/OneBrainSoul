using System.Collections.Generic;

namespace Serialize.NavMeshGraph
{
    [System.Serializable]
    public class SerializableGraph
    {
        public List<SerializableNode> nodes = new List<SerializableNode>();
    }
}