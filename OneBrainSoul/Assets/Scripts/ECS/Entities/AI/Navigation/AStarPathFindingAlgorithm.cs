using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using UnityEngine;
using Utilities;
using Edge = AI.Combat.CombatNavigation.Edge;

namespace ECS.Entities.AI.Navigation
{
    public static class AStarPathFindingAlgorithm
    {
        public static List<Node> FindPath(NavMeshGraph navMeshGraph, Vector3 startPosition, Vector3 goalPosition)
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
                    return CalculateNewPath(goalNode, goalPosition);
                }

                closedSet.Add(currentNode);

                foreach (Edge edge in currentNode.edges)
                {
                    Node neighbor = navMeshGraph.nodes[edge.toNodeIndex];

                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    if (CheckRecursion(currentNode, neighbor))
                    {
                        continue;
                    }
                    
                    float tentativeGScore = currentNode.gCost + edge.cost;

                    if (tentativeGScore > neighbor.gCost)
                    {
                        continue;
                    }
                
                    neighbor.gCost = tentativeGScore;
                    neighbor.hCost = Heuristic(neighbor, goalNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, neighbor.fCost);
                        continue;
                    }
                    
                    openSet.UpdatePriority(neighbor, neighbor.fCost);
                }
            }

            return new List<Node>();
        }

        private static bool CheckRecursion(Node currentNode, Node neighbor)
        {
            Node tempNode = currentNode;
            
            while (tempNode != null)
            {
                if (tempNode.parent != null && tempNode.parent.index == neighbor.index)
                {
                    return true;
                }

                tempNode = tempNode.parent;
            }

            return false;
        }

        private static Node GetClosestNode(NavMeshGraph navMeshGraph, Vector3 position)
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

        private static float Heuristic(Node fromNode, Node toNode)
        {
            return Vector3.Distance(fromNode.position, toNode.position);
        }

        private static List<Node> CalculateNewPath(Node goalNode, Vector3 goalPosition)
        {
            List<Node> path = new List<Node>();

            Node currentNode = goalNode;

            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();
            
            Node lastNode = new Node
            {
                position = goalPosition,
                gCost = path[^1].fCost,
                parent = path[^1]
            };
            path.Add(lastNode);

            return path;
        }
    }
}