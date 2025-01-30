using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Serialize.NavMeshGraph
{
    [MessagePackObject]
    public class SerializableNode
    {
        [Key(0)]
        public uint index;
        
        [Key(1)]
        public Vector3 position;
        
        [Key(2)]
        public int heightIndex;
        
        [Key(3)]
        public List<SerializableEdge> edges = new List<SerializableEdge>();
    }
}