using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Entities.AI.Combat;
using UnityEngine;

namespace ECS.Systems.AI.Navigation
{
    public class UpdateAgentDestinationSystem
    {
        public void UpdateAgentDestination(Vector3 origin, Vector3 destination, AStarPath aStarPath)
        {
            List<Node> newPath = AStarPathFindingAlgorithm.FindPath(aStarPath.navMeshGraph, origin, destination);
            
            aStarPath.navMeshGraph.ResetNodesImportantInfo();

            aStarPath.path = newPath;
            
            aStarPath.path.RemoveAt(0);

            //aStarPath.path = SmoothPath(newPath);

            if (aStarPath.path.Count < 2)
            {
                return;
            }
            aStarPath.path.RemoveAt(aStarPath.path.Count - 2);

            if (aStarPath.path.Count < 2)
            {
                return;
            }
            aStarPath.path.RemoveAt(aStarPath.path.Count - 2);
        }

        private List<Node> SmoothPath(List<Node> originalPath)
        {
            List<Node> smoothedPath = new List<Node>();

            return smoothedPath;
        }
    }
}
