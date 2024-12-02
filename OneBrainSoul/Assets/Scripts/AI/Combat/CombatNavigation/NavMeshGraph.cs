using System.Collections.Generic;
using System.IO;
using Serialize.NavMeshGraph;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Combat.CombatNavigation
{
    public class NavMeshGraph
    {
        public Dictionary<uint, Node> nodes = new Dictionary<uint, Node>();

        public void BuildGraph(NavMeshTriangulation navMeshTriangulation, float triangleArea)
        {
            LoadGraph();
            
            /*Vector3[] vertices = navMeshTriangulation.vertices;
            int[] indices = navMeshTriangulation.indices;

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = indices
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Bounds bounds = mesh.bounds;
            float minWidth = bounds.min.x;
            float maxWidth = bounds.max.x;
            float minDepth = bounds.min.z;
            float maxDepth = bounds.max.z;

            List<Vector3> planeVertices = new List<Vector3>();

            for (float x = bounds.min.x; x < bounds.max.x; x += triangleArea)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += triangleArea)
                {
                    planeVertices.Add(new Vector3(x, 0, z));
                }
            }

            List<int> planeTriangles = new List<int>();
            int rows = Mathf.CeilToInt((maxDepth - minDepth) / triangleArea) + 1;
            int columns = Mathf.CeilToInt((maxWidth - minWidth) / triangleArea) + 1;

            for (int z = 0; z < rows - 1; z++)
            {
                for (int x = 0; x < columns - 1; x++)
                {
                    int topLeft = z * columns + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + columns;
                    int bottomRight = bottomLeft + 1;
                    
                    planeTriangles.Add(topLeft);
                    planeTriangles.Add(bottomLeft);
                    planeTriangles.Add(topRight);
                    
                    planeTriangles.Add(topRight);
                    planeTriangles.Add(bottomLeft);
                    planeTriangles.Add(bottomRight);
                }
            }
            
            foreach (Vector3 vertex in planeVertices)
            {
                NavMeshHit hit;
                if (!NavMesh.SamplePosition(vertex, out hit, Mathf.Infinity, NavMesh.AllAreas))
                {
                    continue;
                }

                if ((1 << hit.mask & NavMesh.GetAreaFromName("Walkable")) != 0)
                {
                    continue;
                }

                uint index = (uint)nodes.Count;

                Node newNode = new Node
                {
                    index = index,
                    position = hit.position
                };
                    
                nodes.Add(index, newNode);
            }

            for (int i = 0; i < planeTriangles.Count; i += 3)
            {
                int v1 = planeTriangles[i];
                int v2 = planeTriangles[i + 1];
                int v3 = planeTriangles[i + 2];

                if (v1 < nodes.Count && v2 < nodes.Count)
                {
                    Node node1 =  nodes[(uint)planeTriangles[i]];
                    Node node2 =  nodes[(uint)planeTriangles[i + 1]];
                    
                    ConnectNodes(node1, node2);
                }

                if (v2 < nodes.Count && v3 < nodes.Count)
                {
                    Node node2 =  nodes[(uint)planeTriangles[i + 1]];
                    Node node3 =  nodes[(uint)planeTriangles[i + 2]];
                    
                    ConnectNodes(node2, node3);
                }

                if (v3 < nodes.Count && v1 < nodes.Count)
                {
                    Node node3 =  nodes[(uint)planeTriangles[i + 2]];
                    Node node1 =  nodes[(uint)planeTriangles[i]];
                    
                    ConnectNodes(node3, node1);
                }
            }
            
            SaveGraph();*/
        }

        private void ConnectTriangleNodes(Node node1, Node node2, Node node3)
        {
            ConnectNodes(node1, node2);
            ConnectNodes(node2, node3);
            ConnectNodes(node3, node1);
        }

        private void ConnectNodes(Node fromNode, Node toNode)
        {
            if (fromNode.edges.Exists(edge => edge.toNode == toNode))
            {
                return;
            }
            
            float cost = Vector3.Distance(fromNode.position, toNode.position);
            
            fromNode.edges.Add(new Edge
            {
                fromNode = fromNode,
                toNode = toNode,
                cost = cost
            });
            
            toNode.edges.Add(new Edge
            {
                fromNode = toNode,
                toNode = fromNode,
                cost = cost
            });
        }

        public void UpdateEdgeWeights(Vector3 obstaclePosition, float radius, float weightMultiplier)
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    if (Vector3.Distance(edge.toNode.position, obstaclePosition) > radius)
                    {
                        continue;
                    }

                    edge.cost *= weightMultiplier;
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

        public void ResetEdgesCost()
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    edge.cost = Vector3.Distance(node.position, edge.toNode.position);
                }
            }
        }

        public NavMeshGraph Copy()
        {
            return new NavMeshGraph { nodes = nodes };
        }

        private void SaveGraph()
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
                        toNodeIndex = edge.toNode.index,
                        cost = edge.cost
                    });
                }
                
                serializableGraph.nodes.Add(serializableNode);
            }

            string json = JsonUtility.ToJson(serializableGraph, true);
            File.WriteAllText(GetFilePath(), json);
            Debug.Log("Graph saved to " + GetFilePath());
        }

        private void LoadGraph()
        {
            string json = File.ReadAllText(GetFilePath());

            SerializableGraph serializableGraph = JsonUtility.FromJson<SerializableGraph>(json);

            Dictionary<uint, Node> nodesByIndex = new Dictionary<uint, Node>();

            foreach (SerializableNode serializableNode in serializableGraph.nodes)
            {
                uint index = serializableNode.index;
                
                Node node = new Node
                {
                    index = index,
                    position = serializableNode.position
                };
                
                nodes.Add(index, node);
                nodesByIndex.Add(index, node);
            }

            foreach (SerializableNode serializableNode in serializableGraph.nodes)
            {
                Node node = nodes[serializableNode.index];

                foreach (SerializableEdge serializableEdge in serializableNode.edges)
                {
                    Node toNode = nodes[serializableEdge.toNodeIndex];
                    
                    node.edges.Add(new Edge
                    {
                        fromNode = node,
                        toNode = toNode,
                        cost = serializableEdge.cost
                    });
                }
            }
        }

        private string GetFilePath()
        {
            return Path.Combine(Application.streamingAssetsPath, "NavMeshGraph.json");
        }
    }
}