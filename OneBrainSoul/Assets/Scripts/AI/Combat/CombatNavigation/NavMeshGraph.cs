using System.Collections.Generic;
using System.IO;
using MessagePack;
using Serialize.NavMeshGraph;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Combat.CombatNavigation
{
    public class NavMeshGraph
    {
        public Dictionary<uint, Node> nodes = new Dictionary<uint, Node>();
        
        public List<Vector3> hitsPositions = new List<Vector3>();

        public NavMeshGraph()
        {
        }

        private NavMeshGraph(Dictionary<uint, Node> nodes)
        {
            this.nodes = nodes;
        }

        public void BuildGraph(NavMeshTriangulation navMeshTriangulation, float triangleSideLength)
        {
            if (File.Exists(GetFilePath()))
            {
                return;
            }
            
            Vector3[] vertices = navMeshTriangulation.vertices;
            int[] indices = navMeshTriangulation.indices;

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = indices
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Bounds bounds = mesh.bounds;

            List<RaycastHit[]> raycastHits = new List<RaycastHit[]>();

            for (float x = bounds.min.x; x < bounds.max.x; x += triangleSideLength)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += triangleSideLength)
                {
                    raycastHits.Add(Physics.RaycastAll(new Vector3(x, bounds.max.y, z), Vector3.down));
                }
            }
            
            NavMeshHit navMeshHit;

            float radius = triangleSideLength * 0.5f;
            
            foreach (RaycastHit[] hits in raycastHits)
            {
                foreach (RaycastHit hit in hits)
                {
                    hitsPositions.Add(hit.point);
                    
                    if (!NavMesh.SamplePosition(hit.point, out navMeshHit, radius, NavMesh.AllAreas))
                    {
                        continue;
                    }

                    uint index = (uint)nodes.Count;

                    Node newNode = new Node
                    {
                        index = index,
                        position = navMeshHit.position
                    };
                    
                    nodes.Add(index, newNode);
                }
            }

            foreach (Node firstNode in nodes.Values)
            {
                foreach (Node secondNode in nodes.Values)
                {
                    if (firstNode == secondNode)
                    {
                        continue;
                    }

                    if (Vector3.Distance(firstNode.position, secondNode.position) >= triangleSideLength * 1.5f)
                    {
                        continue;
                    }
                
                    if (NavMesh.Raycast(firstNode.position, secondNode.position, out navMeshHit, NavMesh.AllAreas))
                    {
                        continue;
                    }
                    
                    ConnectNodes(firstNode, secondNode, triangleSideLength);
                }
            }
            
            SaveGraphFile();
        }

        private void ConnectNodes(Node fromNode, Node toNode, float triangleSideLength)
        {
            if (fromNode.edges.Exists(edge => edge.toNodeIndex == toNode.index))
            {
                return;
            }
            
            float cost = Vector3.Distance(fromNode.position, toNode.position);
            float baseCostMultiplier = cost / triangleSideLength;
            
            fromNode.edges.Add(new Edge
            {
                fromNodeIndex = fromNode.index,
                toNodeIndex = toNode.index,
                cost = cost,
                baseCostMultiplier = baseCostMultiplier
            });
            
            toNode.edges.Add(new Edge
            {
                fromNodeIndex = toNode.index,
                toNodeIndex = fromNode.index,
                cost = cost,
                baseCostMultiplier = baseCostMultiplier
            });
        }

        private Node GetClosestNode(Vector3 position)
        {
            Node closestNode = null;
            
            float minimumDistance = Mathf.Infinity;
            float distance;

            foreach (Node node in nodes.Values)
            {
                distance = Vector3.Distance(node.position, position);

                if (distance >= minimumDistance)
                {
                    continue;
                }

                minimumDistance = distance;
                closestNode = node;
            }

            return closestNode;
        }

        public void UpdateEdgeWeights(uint obstacleID, Vector3 obstaclePosition, float radius, float weightMultiplier)
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    if (Vector3.Distance(nodes[edge.toNodeIndex].position, obstaclePosition) > radius)
                    {
                        continue;
                    }
                    
                    edge.MultiplyDefaultCost(weightMultiplier);
                }
            }
        }

        public void UpdateEdgeWeights(Vector3 obstaclePosition, float radius, float weightMultiplier)
        {
            Vector3 closestNodePosition = GetClosestNode(obstaclePosition).position;
            
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    Node toNode = nodes[edge.toNodeIndex];
                    
                    if (Vector3.Distance(toNode.position, closestNodePosition) > radius)
                    {
                        continue;
                    }
                    
                    edge.MultiplyCost(weightMultiplier);

                    foreach (Edge toNodeEdge in toNode.edges)
                    {
                        if (toNodeEdge.toNodeIndex != node.index)
                        {
                            continue;
                        }

                        if (Vector3.Distance(nodes[toNodeEdge.toNodeIndex].position, closestNodePosition) < radius)
                        {
                            break;
                        }
                        
                        toNodeEdge.MultiplyCost(weightMultiplier);
                    }
                }
            }
        }

        public void ResetEdgesCost()
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    edge.ResetCost();
                }
            }
        }

        public void ResetNodesImportantInfo()
        {
            foreach (Node node in nodes.Values)
            {
                node.gCost = Mathf.Infinity;
                node.parent = null;
            }
        }

        private void SaveGraphFile()
        {
            SerializableGraph serializableGraph = new SerializableGraph();

            foreach (var nodesIndex in nodes)
            {
                Node node = nodesIndex.Value;
                SerializableNode serializableNode = new SerializableNode
                {
                    index = node.index,
                    position = node.position
                };

                foreach (Edge edge in node.edges)
                {
                    serializableNode.edges.Add(new SerializableEdge
                    {
                        toNodeIndex = edge.toNodeIndex,
                        cost = edge.cost,
                        baseCostMultiplier = edge.baseCostMultiplier
                    });
                }
                
                serializableGraph.nodes.Add(serializableNode);
            }

            byte[] data = MessagePackSerializer.Serialize(serializableGraph);
            File.WriteAllBytes(GetFilePath(), data);
        }

        public void EraseGraphFile()
        {
            if (!File.Exists(GetFilePath()))
            {
                return;
            }
            
            File.Delete(GetFilePath());
        }

        public void LoadGraph()
        {
            if (!File.Exists(GetFilePath()))
            {
                Debug.LogError("Missing Graph File");
                return;
            }

            byte[] data = File.ReadAllBytes(GetFilePath());

            SerializableGraph serializableGraph = MessagePackSerializer.Deserialize<SerializableGraph>(data);

            foreach (SerializableNode serializableNode in serializableGraph.nodes)
            {
                uint index = serializableNode.index;
                
                Node node = new Node
                {
                    index = index,
                    position = serializableNode.position
                };
                
                nodes.Add(index, node);
            }

            foreach (SerializableNode serializableNode in serializableGraph.nodes)
            {
                Node node = nodes[serializableNode.index];

                foreach (SerializableEdge serializableEdge in serializableNode.edges)
                {
                    Node toNode = nodes[serializableEdge.toNodeIndex];
                    
                    node.edges.Add(new Edge
                    {
                        fromNodeIndex = node.index,
                        toNodeIndex = toNode.index,
                        distance = Vector3.Distance(nodes[node.index].position, nodes[toNode.index].position),
                        cost = serializableEdge.cost,
                        defaultCost = serializableEdge.cost,
                        baseCostMultiplier = serializableEdge.baseCostMultiplier
                    });
                }
            }
        }

        private string GetFilePath()
        {
            return Path.Combine(Application.streamingAssetsPath, "JSON/NavMeshGraph.json");
        }

        public NavMeshGraph DeepCopy()
        {
            return new NavMeshGraph(nodes);
        }
    }
}