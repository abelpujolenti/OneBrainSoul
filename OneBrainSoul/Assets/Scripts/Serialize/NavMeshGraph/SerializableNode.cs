using System.Collections.Generic;
using UnityEngine;

namespace Serialize.NavMeshGraph
{
    [System.Serializable]
    public class SerializableNode
    {
        public uint index;
        public Vector3 position;
        public List<SerializableEdge> edges = new List<SerializableEdge>();
    }
}