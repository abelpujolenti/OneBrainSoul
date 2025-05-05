using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Entities.AI.Navigation;

namespace ECS.Systems.AI.Navigation
{
    public class UpdateAgentDestinationSystem
    {
        public void UpdateAgentDestination(AStarPath aStarPath, float triangleSideLength)
        {
            aStarPath.path = AStarPathFindingAlgorithm.FindPath(aStarPath.navMeshGraph, aStarPath.origin, 
                aStarPath.destination);
            
            if (aStarPath.path.Count == 0)
            {
                return;
            }

            aStarPath.path = AStarPathFindingAlgorithm.OptimizePath(aStarPath.path, aStarPath.origin,
                aStarPath.navMeshGraph.nodes, triangleSideLength);
        }
    }
}
