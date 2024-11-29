using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Combat.CombatNavigation
{
    public class NavMeshGraph
    {
        public Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();

        public void BuildGraph(NavMeshTriangulation navMeshTriangulation)
        {
            foreach (Vector3 vertex in navMeshTriangulation.vertices)
            {
                if (nodes.ContainsKey(vertex))
                {
                    continue;
                }

                nodes[vertex] = new Node
                {
                    position = vertex
                };
            }

            int indicesLength = navMeshTriangulation.indices.Length;

            for (int i = 0; i < indicesLength; i++)
            {
                Vector3 vector1 = navMeshTriangulation.vertices[navMeshTriangulation.indices[i]];
                Vector3 vector2 = navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 1]];
                Vector3 vector3 = navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 2]];

                ConnectNodes(vector1, vector2);
                ConnectNodes(vector2, vector3);
                ConnectNodes(vector3, vector1);
            }
        }

        private void ConnectNodes(Vector3 fromVector, Vector3 toVector)
        {
            if (!nodes.ContainsKey(fromVector) || !nodes.ContainsKey(toVector))
            {
                return;
            }

            Node fromNode = nodes[fromVector];
            Node toNode = nodes[toVector];

            float cost = Vector3.Distance(fromVector, toVector);
            
            fromNode.neighbors.Add(new Edge
            {
                toNode = toNode,
                cost = cost
            });
            
            toNode.neighbors.Add(new Edge
            {
                toNode = fromNode,
                cost = cost
            });
        }

        public void UpdateEdgeWeights(Vector3 obstaclePosition, float radius, float weightMultiplier)
        {
            foreach (Node node in nodes.Values)
            {
                foreach (Edge edge in node.neighbors)
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
                foreach (Edge edge in node.neighbors)
                {
                    edge.cost = Vector3.Distance(node.position, edge.toNode.position);
                }
            }
        }
    }
}