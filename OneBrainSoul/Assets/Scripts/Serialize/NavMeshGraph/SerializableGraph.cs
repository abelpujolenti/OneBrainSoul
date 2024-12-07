using System.Collections.Generic;
using MessagePack;

namespace Serialize.NavMeshGraph
{
    [MessagePackObject]
    public class SerializableGraph
    {
        [Key(0)]
        public List<SerializableNode> nodes = new List<SerializableNode>();
    }
}