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
                
                Node newNode = new Node
                {
                    position = vertex
                };
                
                nodes.Add(vertex, newNode);
            }

            int indicesLength = navMeshTriangulation.indices.Length;

            for (int i = 0; i < indicesLength; i += 3)
            {
                Node node1 = nodes[navMeshTriangulation.vertices[navMeshTriangulation.indices[i]]];
                Node node2 = nodes[navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 1]]];
                Node node3 = nodes[navMeshTriangulation.vertices[navMeshTriangulation.indices[i + 2]]];

                ConnectNodes(node1, node2);
                ConnectNodes(node2, node3);
                ConnectNodes(node3, node1);
            }
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