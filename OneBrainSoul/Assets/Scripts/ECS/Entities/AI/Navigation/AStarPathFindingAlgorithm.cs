using System;
using System.Collections.Generic;
using System.Linq;
using AI.Combat.CombatNavigation;
using Managers;
using Threads;
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

            Node currentNode;

            startNode.gCost = 0;
            startNode.hCost = Heuristic(startNode, goalNode);
        
            openSet.Enqueue(startNode, startNode.fCost);

            while (openSet.Count != 0)
            {
                currentNode = openSet.Dequeue();

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
        public static List<Node> OptimizePath(List<Node> path, Vector3 origin, Dictionary<uint, Node> nodes, 
            float triangleSideLength)
        {
            path.RemoveAt(0);

            if (path.Count < 3)
            {
                return path;
            }

            path = RemoveNonCorners(path);

            if (path.Count < 3)
            {
                return path;
            }
            
            return RemoveUnnecessaryCorners(path, origin, nodes, triangleSideLength);
        }

        private static List<Node> RemoveNonCorners(List<Node> path)
        {
            Vector3 currentVector;
            Vector3 previousVector = (path[1].position - path[0].position).normalized;
            
            int counter = 1;

            while (counter < path.Count - 1)
            {
                currentVector = (path[counter + 1].position - path[counter].position).normalized;

                if (Math.Abs(Vector3.Dot(previousVector, currentVector) - 1) > 0.01f)
                {
                    previousVector = currentVector;
                    counter++;
                    continue;
                }
                
                path.RemoveAt(counter);
            }

            return path;
        }

        private static List<Node> RemoveUnnecessaryCorners(List<Node> path, Vector3 origin, 
            Dictionary<uint, Node> nodes, float triangleSideLength)
        {
            int counter = 0;

            Vector3 segmentStart;
            Vector3 segmentBetweenNodes;
            
            List<Node> closestNodesToSegment;

            while (counter < path.Count - 2)
            {
                Node startNode = path[counter];

                if (IsAJumpEdge(startNode, path[counter + 1].index))
                {
                    counter++;
                    continue;
                }
                
                Node endNode = path[counter + 2];
                segmentStart = startNode.position;
                segmentBetweenNodes = endNode.position - segmentStart;
                
                closestNodesToSegment = GetClosestNodesToSegment(startNode, nodes, segmentStart, segmentBetweenNodes, triangleSideLength);

                if (!closestNodesToSegment.Contains(endNode) ||
                    !AreEdgesCostsNormal(closestNodesToSegment, segmentStart, segmentBetweenNodes, nodes))
                {
                    counter++;
                    continue;
                }
                
                path.RemoveAt(counter + 1);
            }
            
            path.RemoveAt(0);

            return path;
        }

        private static bool IsAJumpEdge(Node startNode, uint nextNodeIndex)
        {
            foreach (Edge edge in startNode.edges)
            {
                if (edge.toNodeIndex != nextNodeIndex)
                {
                    continue;
                }

                return edge.isAJump;
            }

            return false;
        }

        private static List<Node> GetClosestNodesToSegment(Node startNode, Dictionary<uint, Node> nodes, 
            Vector3 segmentStart, Vector3 segmentBetweenNodes, float maximumDistance)
        {
            float distanceSquared = segmentBetweenNodes.sqrMagnitude;
            
            if (distanceSquared == 0)
            {
                return new List<Node>();
            }

            float closestPointNormalized;

            List<Node> closestNodesToSegment = new List<Node>();
            Queue<Node> openSet = new Queue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            
            openSet.Enqueue(startNode);

            Node currentNode;

            Vector3 pointVector;

            while (openSet.Count != 0)
            {
                currentNode = openSet.Dequeue();

                if (!closedSet.Add(currentNode))
                {
                    continue;
                }

                pointVector = currentNode.position - segmentStart;

                closestPointNormalized = Vector3.Dot(pointVector, segmentBetweenNodes) / distanceSquared;

                if (closestPointNormalized < 0f || closestPointNormalized > 1f)
                {
                    continue;
                }

                foreach (Edge edge in currentNode.edges)
                {
                    Node toNode = nodes[edge.toNodeIndex];
                    openSet.Enqueue(toNode);
                }
                
                closestNodesToSegment.Add(currentNode);
            }
            
            return closestNodesToSegment;
        }

        private static bool AreEdgesCostsNormal(List<Node> closestNodesToSegment, Vector3 segmentStart, Vector3 segmentBetweenNodes, 
            Dictionary<uint, Node> nodes)
        {
            HashSet<Node> leftSideNodes = new HashSet<Node>();
            HashSet<Node> rightSideNodes = new HashSet<Node>();
            
            GetEachSideNodes(closestNodesToSegment, segmentStart, segmentBetweenNodes, ref leftSideNodes, ref rightSideNodes);

            foreach (Node leftSideNode in leftSideNodes)
            {
                foreach (Edge edge in leftSideNode.edges)
                {
                    if (!rightSideNodes.Contains(nodes[edge.toNodeIndex]))
                    {
                        continue;
                    }
                    
                    if (!Mathf.Approximately(edge.cost, edge.defaultCost))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private static void GetEachSideNodes(List<Node> nodes, Vector3 segmentStart, Vector3 segmentBetweenNodes, 
            ref HashSet<Node> leftSideNodes, ref HashSet<Node> rightSideNodes)
        {
            Vector3 perpendicularVector = Vector3.Cross(segmentBetweenNodes, Vector3.up);
            
            Vector3 pointVector;

            foreach (Node node in nodes)
            {
                pointVector = node.position - segmentStart;

                float dotProduct = Vector3.Dot(perpendicularVector, pointVector);

                if (dotProduct > 0)
                {
                    rightSideNodes.Add(node);
                }
                else if (dotProduct < 0)
                {
                    leftSideNodes.Add(node);
                }
            }
        }
    }
}