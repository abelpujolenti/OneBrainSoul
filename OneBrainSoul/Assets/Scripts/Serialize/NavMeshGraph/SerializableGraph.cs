using System;
using System.Collections.Generic;

namespace Serialize.NavMeshGraph
{
    [Serializable]
    public class SerializableGraph
    {
        public List<SerializableNode> nodes = new List<SerializableNode>();
    }
}