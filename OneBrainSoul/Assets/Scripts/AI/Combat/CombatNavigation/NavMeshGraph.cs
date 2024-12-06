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

            List<Vector3> planeVertices = new List<Vector3>();

            for (float x = bounds.min.x; x < bounds.max.x; x += triangleSideLength)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += triangleSideLength)
                {
                    planeVertices.Add(new Vector3(x, 0, z));
                }
            }
            
            NavMeshHit hit;
            
            foreach (Vector3 vertex in planeVertices)
            {
                
                if (!NavMesh.SamplePosition(vertex, out hit, Mathf.Infinity, NavMesh.AllAreas))
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
                
                    if (NavMesh.Raycast(firstNode.position, secondNode.position, out hit, NavMesh.AllAreas))
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
            if (fromNode.edges.Exists(edge => edge.toNode == toNode))
            {
                return;
            }
            
            float cost = Vector3.Distance(fromNode.position, toNode.position);
            float baseCostMultiplier = cost / triangleSideLength;
            
            fromNode.edges.Add(new Edge
            {
                fromNode = fromNode,
                toNode = toNode,
                cost = cost,
                baseCostMultiplier = baseCostMultiplier
            });
            
            toNode.edges.Add(new Edge
            {
                fromNode = toNode,
                toNode = fromNode,
                cost = cost,
                baseCostMultiplier = baseCostMultiplier
            });
        }

        public void UpdateEdgeWeights(uint obstacleID, Vector3 obstaclePosition, float radius, float weightMultiplier)
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    if (Vector3.Distance(edge.toNode.position, obstaclePosition) > radius)
                    {
                        continue;
                    }
                    
                    edge.MultiplyDefaultCost(weightMultiplier);
                }
            }
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

        public void UpdateEdgeWeights(Vector3 obstaclePosition, float radius, float weightMultiplier)
        {
            Vector3 closestNodePosition = GetClosestNode(obstaclePosition).position;
            
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    Node toNode = edge.toNode;
                    
                    if (Vector3.Distance(toNode.position, closestNodePosition) > radius)
                    {
                        continue;
                    }
                    
                    edge.MultiplyCost(weightMultiplier);

                    foreach (Edge toNodeEdge in toNode.edges)
                    {
                        if (toNodeEdge.toNode.index != node.index)
                        {
                            continue;
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
                        toNodeIndex = edge.toNode.index,
                        cost = edge.cost,
                        baseCostMultiplier = edge.baseCostMultiplier
                    });
                }
                
                serializableGraph.nodes.Add(serializableNode);
            }

            string json = JsonUtility.ToJson(serializableGraph, true);
            File.WriteAllText(GetFilePath(), json);
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
                        cost = serializableEdge.cost,
                        defaultCost = serializableEdge.cost,
                        baseCostMultiplier = 1
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