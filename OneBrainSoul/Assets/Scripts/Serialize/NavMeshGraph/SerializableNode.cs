using System;
using System.Collections.Generic;
using UnityEngine;

namespace Serialize.NavMeshGraph
{
    [Serializable]
    public class SerializableNode
    {
        public uint index;
        public Vector3 position;
        public List<SerializableEdge> edges = new List<SerializableEdge>();
    }
}