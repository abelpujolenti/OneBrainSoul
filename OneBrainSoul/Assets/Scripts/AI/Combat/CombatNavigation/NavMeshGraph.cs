using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Combat.CombatNavigation
{
    public class NavMeshGraph
    {
        public Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();

        public void BuildGraph(NavMeshTriangulation navMeshTriangulation, float scalingFactor)
        {
            foreach (Vector3 vertex in navMeshTriangulation.vertices)
            {
                if (nodes.ContainsKey(vertex))
                {
                    continue;
                }
                
                Node newNode = new Node
                {
                    position = vertex
                };
                
                nodes.Add(vertex, newNode);
            }

            int indicesLength = navMeshTriangulation.indices.Length;

            for (int i = 0; i < indicesLength; i += 3)
            {
                Vector3 vertex1 = navMeshTriangulation.vertices[navMeshTriangulation.indices[i]];
                Vector3 vertex2 = navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 1]];
                Vector3 vertex3 = navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 2]];

                float area = CalculateTriangleArea(vertex1, vertex2, vertex3);

                int subdivisions = Mathf.CeilToInt(area * scalingFactor);

                List<Vector3> subdividedVertices = new List<Vector3>();
                List<(int, int, int)> subdividedTriangles = new List<(int, int, int)>();
                
                SubdivideTriangle(vertex1, vertex2, vertex3, subdivisions, subdividedVertices, subdividedTriangles);
                
                foreach (Vector3 vertex in subdividedVertices)
                {
                    if (nodes.ContainsKey(vertex))
                    {
                        continue;
                    }
                
                    Node newNode = new Node
                    {
                        position = vertex
                    };
                
                    nodes.Add(vertex, newNode);
                }
                
                foreach ((int index1, int index2, int index3) in subdividedTriangles)
                {
                    Node node1 = nodes[subdividedVertices[index1]];
                    Node node2 = nodes[subdividedVertices[index2]];
                    Node node3 = nodes[subdividedVertices[index3]];

                    ConnectTriangleNodes(node1, node2, node3);
                }
            }
        }

        private float CalculateTriangleArea(Vector3 node1Position, Vector3 node2Position, Vector3 node3Position)
        {
            return 0.5f * Vector3.Cross(node2Position - node1Position, node3Position - node1Position).magnitude;
        }

        private void SubdivideTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, int subdivisions, 
            List<Vector3> subdividedVertices, List<(int, int, int)> subdividedTriangles)
        {
            Vector3[,] grid = new Vector3[subdivisions + 1,subdivisions + 1];

            for (int i = 0; i <= subdivisions; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float u = (float)(i - j) / subdivisions;
                    float v = (float)j / subdivisions;
                    float w = 1 - u - v;

                    grid[i, j] = u * vertex1 + v * vertex2 + w * vertex3;

                    if (subdividedVertices.Contains(grid[i, j]))
                    {
                        continue;
                    }
                    
                    subdividedVertices.Add(grid[i, j]);
                }
            }
            
            for (int i = 0; i < subdivisions; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    int current = subdividedVertices.IndexOf(grid[i, j]);
                    int below = subdividedVertices.IndexOf(grid[i + 1, j]);
                    int belowRight = subdividedVertices.IndexOf(grid[i + 1, j + 1]);
                    int right = j < i ? subdividedVertices.IndexOf(grid[i, j + 1]) : -1;

                    subdividedTriangles.Add((current, below, belowRight));

                    if (right == -1)
                    {
                        continue;
                    }
                    
                    subdividedTriangles.Add((current, belowRight, right));
                }
            }
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

        public void ResetEdgeWeights()
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.edges)
                {
                    edge.cost = Vector3.Distance(node.position, edge.toNode.position);
                }
            }
        }
    }
}