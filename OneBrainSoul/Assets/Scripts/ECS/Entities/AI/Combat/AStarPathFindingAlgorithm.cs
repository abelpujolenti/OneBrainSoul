using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class AStarPathFindingAlgorithm
    {
        /*public List<Vector3> FindPath(NavMeshGraph navMeshGraph, Vector3 startPosition, Vector3 goalPosition)
        {
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            Dictionary<Node, float> gScore = new Dictionary<Node, float>();
            Dictionary<Node, float> fScore = new Dictionary<Node, float>();

            Node startNode = GetClosestNode(navMeshGraph, startPosition);
            Node goalNode = GetClosestNode(navMeshGraph, goalPosition);

            foreach (Node node in navMeshGraph.nodes.Values)
            {
                gScore[node] = Mathf.Infinity;
                fScore[node] = Mathf.Infinity;
            }

            gScore[startNode] = 0;
            fScore[startNode] = Heuristic(startNode.position, goalNode.position);
        }*/

        private Node GetClosestNode(NavMeshGraph navMeshGraph, Vector3 position)
        {
            Node closestNode = null;

            float minimumDistance = Mathf.Infinity;
            float distance;

            foreach (Node node in navMeshGraph.nodes.Values)
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

        private float Heuristic(Vector3 fromVector, Vector3 toVector)
        {
            return Vector3.Distance(fromVector, toVector);
        }

        private List<Vector3> CalculateNewPath(Dictionary<Node, Node> cameFrom, Node current)
        {
            List<Vector3> newPath = new List<Vector3>
            {
                current.position
            };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                newPath.Insert(0, current.position);
            }

            return newPath;
        }
    }
}