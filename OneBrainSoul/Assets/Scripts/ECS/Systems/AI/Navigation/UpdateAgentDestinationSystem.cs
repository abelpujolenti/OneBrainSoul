using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Entities.AI.Combat;
using Managers;
using Threads;

namespace ECS.Systems.AI.Navigation
{
    public class UpdateAgentDestinationSystem
    {
        public void UpdateAgentDestination(AStarPath aStarPath, float triangleSideLength, MainThreadQueue mainThreadQueue)
        {
            List<Node> newPath = AStarPathFindingAlgorithm.FindPath(aStarPath.navMeshGraph, aStarPath.origin, 
                aStarPath.destination);

            if (newPath.Count == 0)
            {
                return;
            }
            
            mainThreadQueue.SetAction(() => ECSNavigationManager.Instance.DEBUG_PassOriginalPathToAgent(newPath));

            List<Node> optimizedPath = new List<Node>();
            
            optimizedPath.AddRange(newPath);

            aStarPath.path = AStarPathFindingAlgorithm.OptimizePath(optimizedPath, aStarPath.origin,
                aStarPath.navMeshGraph.nodes, triangleSideLength, mainThreadQueue);
            
            mainThreadQueue.SetAction(() => ECSNavigationManager.Instance.DEBUG_PassOptimizedPathToAgent(optimizedPath));
        }
    }
}
