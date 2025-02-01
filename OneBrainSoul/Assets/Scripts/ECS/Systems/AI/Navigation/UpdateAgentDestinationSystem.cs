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
            List<Node> newPath = AStarPathFindingAlgorithm.FindPath(aStarPath.navMeshGraph, aStarPath.origin, 
                aStarPath.destination);

            if (newPath.Count == 0)
            {
                return;
            }

            aStarPath.path = AStarPathFindingAlgorithm.OptimizePath(newPath, aStarPath.origin, 
                aStarPath.navMeshGraph.nodes, triangleSideLength);
            
            aStarPath.path.RemoveAt(0);

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
    }
}
