using System;
using System.Collections.Generic;
using System.Linq;
using AI.Combat.CombatNavigation;
using Managers;
using Threads;
using UnityEngine;
using Utilities;
using Edge = AI.Combat.CombatNavigation.Edge;

namespace ECS.Entities.AI.Combat
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
            float triangleSideLength, MainThreadQueue mainThreadQueue)
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

            return RemoveUnnecessaryCorners(path, origin, nodes, triangleSideLength, mainThreadQueue);
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
            Dictionary<uint, Node> nodes, float triangleSideLength, MainThreadQueue mainThreadQueue)
        {
            path.Insert(0, new Node
            {
                position = origin
            });
            
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
                segmentBetweenNodes = endNode.position - startNode.position;
                
                closestNodesToSegment = GetClosestNodesToSegment(startNode, nodes, segmentStart, segmentBetweenNodes, 
                    triangleSideLength, mainThreadQueue);

                HashSet<Node> leftSideNodes;
                HashSet<Node> rightSideNodes;

                if (!closestNodesToSegment.Contains(endNode.parent) ||
                    ExistHoles(closestNodesToSegment, segmentStart, segmentBetweenNodes, nodes, 
                        out leftSideNodes, out rightSideNodes)/* || 
                    CheckEdgesCosts(closestNodesToSegment, segmentStart, segmentBetweenNodes)*/)
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
            int i = 0;

            bool matched = false;

            for (; i < startNode.edges.Count; i++)
            {
                if (startNode.edges[i].toNodeIndex != nextNodeIndex)
                {
                    continue;
                }

                matched = true;
                break;
            }

            return matched && startNode.edges[i].isAJump;
        }

        private static List<Node> GetClosestNodesToSegment(Node startNode, Dictionary<uint, Node> nodes, 
            Vector3 segmentStart, Vector3 segmentBetweenNodes, float maximumDistance, MainThreadQueue mainThreadQueue)
        {
            float segmentLengthSquared = Vector3.Dot(segmentBetweenNodes, segmentBetweenNodes);
            
            if (segmentLengthSquared == 0)
            {
                return new List<Node>();
            }
            
            mainThreadQueue.SetAction(() => ECSNavigationManager.Instance.DEBUG_PassCubesToAgent(segmentStart, segmentBetweenNodes.normalized, 
                segmentBetweenNodes.magnitude, maximumDistance * 2));

            float closestPointNormalized;

            List<Node> closestNodesToSegment = new List<Node>();
            PriorityQueue<Node> openSet = new PriorityQueue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            
            openSet.Enqueue(startNode, 0);

            Node currentNode;

            Vector3 pointVector;
            Vector3 closestPoint;

            while (openSet.Count != 0)
            {
                currentNode = openSet.Dequeue();

                closedSet.Add(currentNode);

                pointVector = currentNode.position - segmentStart;

                closestPointNormalized = Vector3.Dot(pointVector, segmentBetweenNodes) / segmentLengthSquared; 

                if (closestPointNormalized < 0f || closestPointNormalized > 1f)
                {
                    continue;
                }

                closestPoint = segmentStart + closestPointNormalized * segmentBetweenNodes;

                if ((currentNode.position - closestPoint).sqrMagnitude > maximumDistance * maximumDistance)
                {
                    continue;
                }
                
                closestNodesToSegment.Add(currentNode);

                foreach (Edge edge in currentNode.edges)
                {
                    Node neighbor = nodes[edge.toNodeIndex];
                    
                    if (closedSet.Contains(neighbor) || openSet.Contains(neighbor))
                    {
                        continue;
                    }
                    
                    openSet.Enqueue(neighbor, (segmentStart - neighbor.position).sqrMagnitude);
                }
            }
            
            mainThreadQueue.SetAction(() => ECSNavigationManager.Instance.DEBUG_PassClosestNodesToAgent(closestNodesToSegment));
            
            return closestNodesToSegment;
        }

        private static bool ExistHoles(List<Node> closestNodesToSegment, Vector3 segmentStart, Vector3 segmentBetweenNodes, 
            Dictionary<uint, Node> nodes, out HashSet<Node> leftSideNodes, out HashSet<Node> rightSideNodes)
        {
            GetEachSideNodes(closestNodesToSegment, segmentStart, segmentBetweenNodes, out leftSideNodes, out rightSideNodes);

            if (leftSideNodes.Count == 0 || rightSideNodes.Count == 0)
            {
                return false;
            }

            if (!AllSideIsInterconnected(leftSideNodes, nodes))
            {
                return true;
            }
            
            if (!AllSideIsInterconnected(rightSideNodes, nodes))
            {
                return true;
            }

            return !ExistConnectionsBetweenSides(leftSideNodes, rightSideNodes, nodes);
        }

        private static void GetEachSideNodes(List<Node> nodes, Vector3 segmentStart, Vector3 segmentBetweenNodes, 
            out HashSet<Node> leftSideNodes, out HashSet<Node> rightSideNodes)
        {
            leftSideNodes = new HashSet<Node>();
            rightSideNodes = new HashSet<Node>();
            
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

        private static bool AllSideIsInterconnected(HashSet<Node> sideNodes, Dictionary<uint, Node> nodes)
        {
            Queue<Node> openSet = new Queue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            
            openSet.Enqueue(sideNodes.First());

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.Dequeue();

                closedSet.Add(currentNode);

                foreach (Edge edge in currentNode.edges)
                {
                    Node toNode = nodes[edge.toNodeIndex];

                    if (!sideNodes.Contains(toNode))
                    {
                        continue;
                    }

                    if (closedSet.Contains(toNode))
                    {
                        continue;
                    }
                    
                    openSet.Enqueue(toNode);
                }
            }
            
            return closedSet.Count == sideNodes.Count;
        }

        private static bool ExistConnectionsBetweenSides(HashSet<Node> leftSide, HashSet<Node> rightSide, 
            Dictionary<uint, Node> nodes)
        {
            uint counter = 0;
            
            foreach (Node node in rightSide)
            {
                foreach (Edge edge in node.edges)
                {
                    if (!leftSide.Contains(nodes[edge.toNodeIndex]))
                    {
                        continue;
                    }

                    counter++;
                }

                if (counter < 2)
                {
                    return false;    
                }

                counter = 0;
            }
            return true;
        }

        private static bool CheckEdgesCosts(List<Node> nodes, Vector3 segmentStart, Vector3 segmentBetweenNodes)
        {
            return true;
        }
    }
}