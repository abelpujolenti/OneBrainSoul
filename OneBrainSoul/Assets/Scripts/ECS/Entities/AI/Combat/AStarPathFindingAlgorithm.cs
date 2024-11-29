using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using UnityEngine;
using Utilities;
using Edge = AI.Combat.CombatNavigation.Edge;

namespace ECS.Entities.AI.Combat
{
    public class AStarPathFindingAlgorithm
    {
        public List<Vector3> FindPath(NavMeshGraph navMeshGraph, Vector3 startPosition, Vector3 goalPosition)
        {
            PriorityQueue<Node> openSet = new PriorityQueue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            Node startNode = GetClosestNode(navMeshGraph, startPosition);
            Node goalNode = GetClosestNode(navMeshGraph, goalPosition);

            startNode.gCost = 0;
            startNode.hCost = Heuristic(startNode, goalNode);
            
            openSet.Enqueue(startNode, startNode.fCost);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.Dequeue();

                if (currentNode == goalNode)
                {
                    List<Vector3> newPath = CalculateNewPath(goalNode);
                    newPath.Add(goalPosition);
                    return newPath;
                }

                closedSet.Add(currentNode);

                foreach (Edge edge in currentNode.edges)
                {
                    Node neighbor = edge.toNode;

                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeGScore = currentNode.gCost + edge.cost;

                    if (tentativeGScore >= neighbor.gCost)
                    {
                        continue;
                    }
                    
                    neighbor.gCost = tentativeGScore;
                    neighbor.hCost = Heuristic(neighbor, goalNode);
                    neighbor.parent = currentNode;

                    if (openSet.Contains(neighbor))
                    {
                        continue;
                    }
                        
                    openSet.Enqueue(neighbor, neighbor.fCost);
                }
            }

            return new List<Vector3>();
        }

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

        private float Heuristic(Node fromNode, Node toNode)
        {
            return Vector3.Distance(fromNode.position, toNode.position);
        }

        private List<Vector3> CalculateNewPath(Node goalNode)
        {
            List<Vector3> path = new List<Vector3>();

            Node currentNode = goalNode;

            while (currentNode != null)
            {
                path.Add(currentNode.position);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();

            return path;
        }
    }
}